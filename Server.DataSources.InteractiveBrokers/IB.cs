// And unless you have subscribed real time data there is no way to use IB as real time data streamer.
// No delayed data stream on demo account nor on paper account.

using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Interfaces;
using Common.Properties;
using Common.Requests;
using Common.Utils;
using Krs.Ats.IBNet;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Threading;
using BarSize = Common.Enums.BarSize;
using HistoricalDataEventArgs = Common.EventArguments.HistoricalDataEventArgs;
using LogLevel = NLog.LogLevel;

namespace DataSource.InteractiveBrokers
{
    public class IB : IHistoricalDataSource, IRealTimeDataSource
    {
        private readonly ConcurrentDictionary<int, List<OHLCBar>> arrivedHistoricalData;
        private readonly IIBClient client;
        private readonly int clientID;
        private readonly ConcurrentDictionary<int, HistoricalDataRequest> historicalDataRequests;
        private readonly Queue<int> historicalRequestQueue;

        private readonly string host;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly int port;
        private readonly object queueLock = new object();
        private readonly Dictionary<int, RealTimeDataRequest> realTimeDataRequests;

        private readonly Queue<int> realTimeRequestQueue;

        /// <summary>
        ///     Connects two IDs: the AssignedID of the RealTimeDataRequest from the broker, and the ID of the
        ///     request at the TWS client.
        ///     Key: tws client ID, value: AssignedID
        /// </summary>
        private readonly Dictionary<int, int> requestIDMap;

        private readonly object requestIDMapLock = new object();
        private readonly object subReqMapLock = new object();

        /// <summary>
        ///     This holds the number of outstanding sub requests.
        ///     Key: original request ID, Value: the number of subrequests sent out but not returned.
        /// </summary>
        private readonly Dictionary<int, int> subRequestCount;

        /// <summary>
        ///     Sub-requests are created when we need to send multiple requests to the
        ///     IB client to fulfill a single data request. This one holds the ID mappings between them.
        ///     Key: sub-request ID, Value: the ID of the original request that generated it.
        /// </summary>
        private readonly Dictionary<int, int> subRequestIDMap;

        private bool connected;

        /// <summary>
        ///     Periodically updates the Connected property.
        /// </summary>
        private Timer connectionStatusUpdateTimer;

        private int requestCounter;

        /// <summary>
        ///     Used to repeat failed requests after some time has passed.
        /// </summary>
        private Timer requestRepeatTimer;

        public IB(string host, int port, int clientID = -1, IIBClient client = null)
        {
            Name = "Interactive Brokers";

            if (clientID < 0)
                clientID = 99;
            this.host = host;
            this.port = port;
            this.clientID = clientID;

            realTimeDataRequests = new Dictionary<int, RealTimeDataRequest>();
            realTimeRequestQueue = new Queue<int>();

            historicalDataRequests = new ConcurrentDictionary<int, HistoricalDataRequest>();
            historicalRequestQueue = new Queue<int>();

            arrivedHistoricalData = new ConcurrentDictionary<int, List<OHLCBar>>();

            subRequestIDMap = new Dictionary<int, int>();
            subRequestCount = new Dictionary<int, int>();
            requestIDMap = new Dictionary<int, int>();

            requestRepeatTimer = new Timer(20000); //we wait 20 seconds to repeat failed requests
            requestRepeatTimer.Elapsed += ReSendRequests;

            connectionStatusUpdateTimer = new Timer(1000);
            connectionStatusUpdateTimer.Elapsed += _connectionStatusUpdateTimer_Elapsed;
            connectionStatusUpdateTimer.Start();

            requestCounter = 1;

            this.client = client ?? new IBClient();
            this.client.Error += _client_Error;
            this.client.ConnectionClosed += _client_ConnectionClosed;
            this.client.RealTimeBar += _client_RealTimeBar;
            this.client.HistoricalData += _client_HistoricalData;
        }

        #region IHistoricalDataSource Members

        public string Name { get; }

        public bool Connected
        {
            get { return connected; }

            private set
            {
                connected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     historical data request
        /// </summary>
        public void RequestHistoricalData(HistoricalDataRequest request)
        {
            //Historical data limitations: https://www.interactivebrokers.com/en/software/api/apiguide/api/historical_data_limitations.htm
            //the issue here is that the request may not be fulfilled...so we need to keep track of the request
            //and if we get an error regarding its failure, send it again using a timer
            var originalReqID = ++requestCounter;
            historicalDataRequests.TryAdd(originalReqID, request);

            arrivedHistoricalData.TryAdd(originalReqID, new List<OHLCBar>());

            //if necessary, chop up the request into multiple chunks so as to abide
            //the historical data limitations
            if (TwsUtils.RequestObeysLimits(request))
            {
                //send the request, no need for subrequests
                SendHistoricalRequest(originalReqID, request);
            }
            else
            {
                //create subrequests, add them to the ID map, and send them to TWS
                var subRequests = SplitRequest(request);
                subRequestCount.Add(originalReqID, subRequests.Count);

                foreach (var subReq in subRequests)
                {
                    lock (subReqMapLock)
                    {
                        requestCounter++;
                        historicalDataRequests.TryAdd(requestCounter, subReq);
                        subRequestIDMap.Add(requestCounter, originalReqID);
                        SendHistoricalRequest(requestCounter, subReq);
                    }
                }
            }
        }

        public void Connect()
        {
            try
            {
                client.Connect(host, port, clientID);
            }
            catch (Exception e)
            {
                RaiseEvent(Error, this, new ErrorArgs(0, e.Message));
            }
            requestRepeatTimer.Start();
        }

        public void Disconnect()
        {
            client.Disconnect();
            requestRepeatTimer.Stop();
        }

        public event EventHandler<ErrorArgs> Error;

        public event EventHandler<DataSourceDisconnectEventArgs> Disconnected;

        public event EventHandler<HistoricalDataEventArgs> HistoricalDataArrived;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion IHistoricalDataSource Members

        #region IRealTimeDataSource Members

        /// <summary>
        ///     real time data request
        /// </summary>
        public int RequestRealTimeData(RealTimeDataRequest request)
        {
            lock (requestIDMapLock)
            {
                requestCounter++;
                realTimeDataRequests.Add(requestCounter, request);
                requestIDMap.Add(requestCounter, request.AssignedID);
            }

            try
            {
                var contract = TwsUtils.InstrumentToContract(request.Instrument);

                client.RequestRealTimeBars(
                    requestCounter,
                    contract,
                    (int)TwsUtils.BarSizeConverter(request.Frequency),
                    RealTimeBarType.Trades, request.RTHOnly);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "IB: Could not send real time data request: " + ex.Message);
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not send real time data request: " + ex.Message));
            }
            return requestCounter;
        }

        public void CancelRealTimeData(int requestID)
        {
            client.CancelRealTimeBars(requestID);
        }

        public event EventHandler<RealTimeDataEventArgs> DataReceived;

        #endregion IRealTimeDataSource Members

        /// <summary>
        ///     Update the connection status.
        /// </summary>
        private void _connectionStatusUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Connected = Dispatcher.CurrentDispatcher.Invoke(() => client?.Connected ?? false);
        }

        /// <summary>
        ///     This event is raised when historical data arrives from TWS
        /// </summary>
        private void _client_HistoricalData(object sender, Krs.Ats.IBNet.HistoricalDataEventArgs e)
        {
            //convert the bar and add it to the Dictionary of arrived data
            var bar = TwsUtils.HistoricalDataEventArgsToOHLCBar(e);
            int id;

            lock (subReqMapLock)
            {
                //if the data is arriving for a sub-request, we must get the id of the original request first
                //otherwise it's just the same id
                id = subRequestIDMap.ContainsKey(e.RequestId)
                    ? subRequestIDMap[e.RequestId]
                    : e.RequestId;
            }

            //stocks need to have their volumes multiplied by 100, I think all other instrument types do not
            if (historicalDataRequests[id].Instrument.Type == InstrumentType.Stock)
                bar.Volume *= 100;

            arrivedHistoricalData[id].Add(bar);

            if (e.RecordNumber >= e.RecordTotal - 1)
            //this was the last item to receive for this request, send it to the broker
            {
                var requestComplete = true;

                lock (subReqMapLock)
                {
                    if (subRequestIDMap.ContainsKey(e.RequestId))
                    {
                        //If there are sub-requests, here we check if this is the last one
                        requestComplete = ControlSubRequest(e.RequestId);

                        if (requestComplete)
                        {
                            //If it was the last one, we need to order the data because sub-requests can arrive out of order
                            arrivedHistoricalData[id] = arrivedHistoricalData[id].OrderBy(x => x.DateTimeOpen).ToList();
                        }
                    }
                }

                if (requestComplete)
                    HistoricalDataRequestComplete(id);
            }
        }

        /// <summary>
        ///     Control the dictionaries of subRequests
        /// </summary>
        /// <param name="subId">SubRequestID</param>
        /// <returns>Returns true if the parent request is complete</returns>
        private bool ControlSubRequest(int subId)
        {
            var requestComplete = false;

            lock (subReqMapLock)
            {
                if (subRequestIDMap.ContainsKey(subId))
                {
                    var originalID = subRequestIDMap[subId];
                    subRequestIDMap.Remove(subId);
                    subRequestCount[originalID]--;
                    if (subRequestCount[originalID] > 0)
                    {
                        //What happens here: this is a subrequest.
                        //We check how many sub-requests in this group have been delivered.
                        //if this is the last one, we want to call HistoricalDataRequestComplete()
                        //otherwise there's more data to come, so we have to wait for it
                    }
                    else
                        requestComplete = true;
                }
            }

            return requestComplete;
        }

        /// <summary>
        ///     This method is called when a historical data request has delivered all its bars
        /// </summary>
        /// <param name="requestID"></param>
        private void HistoricalDataRequestComplete(int requestID)
        {
            var request = historicalDataRequests[requestID];

            //IB doesn't actually allow us to provide a deterministic starting point for our historical data query
            //so sometimes we get data from earlier than we want
            //here we throw it away
            var cutoffDate = request.StartingDate.Date;

            var removed = arrivedHistoricalData.TryRemove(requestID, out List<OHLCBar> bars);
            if (!removed) return;

            //due to the nature of sub-requests, the list may contain the same points multiple times
            //so we grab unique values only
            bars = bars.Distinct((x, y) => x.DateTimeOpen == y.DateTimeOpen).ToList();

            //we have to make adjustments to the times as well as derive the bar closing times
            AdjustBarTimes(bars, request);

            //return the data through the HistoricalDataArrived event
            RaiseEvent(HistoricalDataArrived, this, new HistoricalDataEventArgs(
                request,
                bars.Where(x => x.DateTimeClose.Date >= cutoffDate).ToList()));
        }

        /// <summary>
        /// </summary>
        private static void AdjustBarTimes(List<OHLCBar> bars, HistoricalDataRequest request)
        {
            //One day or lower frequency means we don't get time data.
            //Instead we provide our own by using that day's session end...
            if (request.Frequency < BarSize.OneDay)
            {
                AdjustIntradayBarTimes(bars, request);
                GenerateIntradayBarClosingTimes(bars, request.Frequency);
            }
            else
                AdjustDailyBarTimes(bars);
        }

        /// <summary>
        ///     Simply converts the timezone, from the local to that of the exchange.
        /// </summary>
        private static void AdjustIntradayBarTimes(IEnumerable<OHLCBar> bars, HistoricalDataRequest request)
        {
            TimeZoneInfo exchangeTz = TimeZoneInfo.FindSystemTimeZoneById(request.Instrument.Exchange.Timezone);
            foreach (OHLCBar bar in bars)
            {
                if (bar.DateTimeOpen != null)
                    bar.DateTimeOpen = TimeZoneInfo.ConvertTime(bar.DateTimeOpen.Value, TimeZoneInfo.Local, exchangeTz);
            }
        }

        /// <summary>
        ///     Simply converts the timezone, from the local to that of the exchange.
        /// </summary>
        private static void AdjustDailyBarTimes(IEnumerable<OHLCBar> bars)
        {
            // For daily data, IB does not provide us with bar opening/closing times.
            // But the IB client does shift the timezone from UTC to local.
            // So to get the correct day we have to shift it back to UTC first.
            foreach (var bar in bars)
            {
                if (bar.DateTimeOpen != null)
                {
                    bar.DateTimeOpen = TimeZoneInfo.ConvertTimeToUtc(bar.DateTimeOpen.Value, TimeZoneInfo.Local);
                    bar.DateTimeClose = bar.DateTimeOpen.Value;
                }
            }
        }

        /// <summary>
        ///     Sets the appropriate closing time for each bar, since IB only gives us the opening time.
        /// </summary>
        private static void GenerateIntradayBarClosingTimes(List<OHLCBar> bars, BarSize frequency)
        {
            var freqTs = frequency.ToTimeSpan();
            for (var i = 0; i < bars.Count; i++)
            {
                var bar = bars[i];

                if (i == bars.Count - 1)
                {
                    //if it's the last bar we are basically just guessing the
                    //closing time by adding the duration of the frequency
                    if (bar.DateTimeOpen != null) bar.DateTimeClose = bar.DateTimeOpen.Value + freqTs;
                }
                else
                {
                    //if it's not the last bar, we set the closing time to the
                    //earliest of the open of the next bar and the period of the frequency
                    //e.g. if hourly bar opens at 9:30 and the next bar opens at 10:00
                    //we set the close at the earliest of 10:00 and 10:30
                    if (bar.DateTimeOpen != null)
                    {
                        var openPlusBarSize = bar.DateTimeOpen.Value + freqTs;
                        DateTime? dateTime = bars[i + 1].DateTimeOpen;

                        if (dateTime != null)
                            bar.DateTimeClose = dateTime.Value < openPlusBarSize ? dateTime.Value : openPlusBarSize;
                    }
                }
            }
        }

        //This event is raised when real time data arrives
        //We convert them and pass them on downstream
        private void _client_RealTimeBar(object sender, RealTimeBarEventArgs e)
        {
            var args = TwsUtils.RealTimeDataEventArgsConverter(e);
            var originalRequest = realTimeDataRequests[e.RequestId];
            if (originalRequest?.Instrument!= null) args.InstrumentID = originalRequest.Instrument.ID;
            args.RequestID = requestIDMap[e.RequestId];
            RaiseEvent(DataReceived, this, args);
        }

        //This event is raised when the connection to TWS client closed.
        private void _client_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            RaiseEvent(Disconnected, this, new DataSourceDisconnectEventArgs(Name, ""));
        }

        /// <summary>
        ///     This event is raised in the case of some error
        ///     This includes pacing violations, in which case we re-enqueue the request.
        /// </summary>
        private void _client_Error(object sender, ErrorEventArgs e)
        {
            //if we asked for too much real time data at once, we need to re-queue the failed request
            if ((int)e.ErrorCode == 420) //a real time pacing violation
            {
                if (!e.ErrorMsg.Contains("No market data permissions"))
                {
                    lock (queueLock)
                    {
                        if (!realTimeRequestQueue.Contains(e.TickerId))
                        {
                            //since the request did not succeed, what we do is re-queue it and it gets requested again by the timer
                            realTimeRequestQueue.Enqueue(e.TickerId);
                        }
                    }
                }
            }
            else if ((int)e.ErrorCode == 162) //a historical data pacing violation
            {
                if (e.ErrorMsg.StartsWith("Historical Market Data Service error message:HMDS query returned no data"))

                {
                    //no data returned = we return an empty data set
                    int origId;

                    lock (subReqMapLock)
                    {
                        //if the data is arriving for a sub-request, we must get the id of the original request first
                        //otherwise it's just the same id
                        origId = subRequestIDMap.ContainsKey(e.TickerId)
                            ? subRequestIDMap[e.TickerId]
                            : e.TickerId;
                    }

                    if (origId != e.TickerId)
                    {
                        //this is a subrequest - only complete the
                        if (ControlSubRequest(e.TickerId))
                            HistoricalDataRequestComplete(origId);
                    }
                    else
                        HistoricalDataRequestComplete(origId);
                }
                else
                {
                    //simply a data pacing violation
                    lock (queueLock)
                    {
                        if (!historicalRequestQueue.Contains(e.TickerId))
                        {
                            //same as above
                            historicalRequestQueue.Enqueue(e.TickerId);
                        }
                    }
                }
            }
            else if ((int)e.ErrorCode == 200) //No security definition has been found for the request.
            {
                //Again multiple errors share the same code...
                if (e.ErrorMsg.Contains("No security definition has been found for the request"))
                {
                    //this will happen for example when asking for data on expired futures
                    //return an empty data list
                    if (historicalDataRequests.ContainsKey(e.TickerId))
                        HistoricalDataRequestComplete(e.TickerId);
                }
                else //in this case we're handling a "Invalid destination exchange specified" error
                {
                    //not sure if there's anything else to do, if it's a real time request it just fails...
                    if (historicalDataRequests.ContainsKey(e.TickerId))
                        HistoricalDataRequestComplete(e.TickerId);
                }
            }

            RaiseErrorMessage(e);
        }

        private void RaiseErrorMessage(ErrorEventArgs e)
        {
            var errorArgs = TwsUtils.ConvertErrorArguments(e);
            var isHistorical = historicalDataRequests.TryGetValue(e.TickerId, out HistoricalDataRequest histReq);
            if (isHistorical)
            {
                var origId = subRequestIDMap.ContainsKey(histReq.RequestID)
                    ? subRequestIDMap[histReq.RequestID]
                    : histReq.RequestID;

                errorArgs.ErrorMessage +=
                    $" Historical Req: {histReq.Instrument.Symbol} @ {histReq.Frequency} From {histReq.StartingDate} To {histReq.EndingDate} - TickerId: {e.TickerId}  ReqID: {histReq.RequestID}";

                errorArgs.RequestID = origId;
            }
            else if (realTimeDataRequests.TryGetValue(e.TickerId, out RealTimeDataRequest rtReq)) //it's a real time request
            {
                errorArgs.ErrorMessage += $" RT Req: {rtReq.Instrument.Symbol} @ {rtReq.Frequency}";

                errorArgs.RequestID = rtReq.RequestID;
            }

            RaiseEvent(Error, this, errorArgs);
        }

        /// <summary>
        ///     Splits a historical data request into multiple pieces so that they obey the request limits
        /// </summary>
        private List<HistoricalDataRequest> SplitRequest(HistoricalDataRequest request)
        {
            var requests = new List<HistoricalDataRequest>();

            //start at the end, and work backward in increments slightly lower than the max allowed time
            var step = (int)(TwsUtils.MaxRequestLength(request.Frequency) * .95);
            var currentDate = request.EndingDate;
            while (currentDate > request.StartingDate)
            {
                var newReq = request;
                newReq.EndingDate = currentDate;
                newReq.StartingDate = newReq.EndingDate.AddSeconds(-step);
                if (newReq.StartingDate < request.StartingDate)
                    newReq.StartingDate = request.StartingDate;

                currentDate = currentDate.AddSeconds(-step);
                requests.Add(newReq);
            }

            return requests;
        }

        private void SendHistoricalRequest(int id, HistoricalDataRequest request)
        {
            Log(LogLevel.Info,
                $"Sent historical data request to TWS. ID: {id}, Symbol: {request.Instrument.Symbol}, {TwsUtils.TimespanToDurationString(request.EndingDate - request.StartingDate, request.Frequency)} back from {request.EndingDate:yyyy-MM-dd hh:mm:ss}");

            var exchangeTz = request.Instrument.GetTimeZoneInfo();
            //we need to convert time from the exchange TZ to Local...the ib client then converts it to UTC
            var startingDate = TimeZoneInfo.ConvertTime(request.StartingDate, exchangeTz, TimeZoneInfo.Local);
            var endingDate = TimeZoneInfo.ConvertTime(request.EndingDate, exchangeTz, TimeZoneInfo.Local);

            try
            {
                client.RequestHistoricalData
                (
                    id,
                    TwsUtils.InstrumentToContract(request.Instrument),
                    endingDate,
                   TwsUtils.TimespanToDurationString(endingDate - startingDate, request.Frequency),
                    TwsUtils.BarSizeConverter(request.Frequency),
                    GetDataType(),
                    request.RTHOnly ? 1 : 0
                );
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "IB: Could not send historical data request: " + ex.Message);
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not send historical data request: " + ex.Message, id));
            }
        }

        private HistoricalDataType GetDataType() => HistoricalDataType.Trades;

        /// <summary>
        ///     Add a message to the log.
        /// </summary>
        private void Log(LogLevel level, string message)
        {
            logger.Log(level, message);
        }

        /// <summary>
        ///     This event is raised when the _requestRepeatTimer period elapses.
        ///     It repeats failed requests for historical or real time data
        /// </summary>
        private void ReSendRequests(object sender, ElapsedEventArgs e)
        {
            if (!client.Connected) return;
            lock (queueLock)
            {
                while (realTimeRequestQueue.Count > 0)
                {
                    var requestID = realTimeRequestQueue.Dequeue();
                    var symbol =
                        realTimeDataRequests[requestID].Instrument.Symbol;
                      
                    RequestRealTimeData(realTimeDataRequests[requestID]);
                    Log(LogLevel.Info,
                        $"IB Repeating real time data request for {symbol} @ {realTimeDataRequests[requestID].Frequency}");
                }

                while (historicalRequestQueue.Count > 0)
                {
                    var requestID = historicalRequestQueue.Dequeue();

                    var symbol = historicalDataRequests[requestID].Instrument.Symbol;
                    

                    // We repeat the request _with the same id_ as used previously. This means the previous
                    // sub request mapping will still work
                    SendHistoricalRequest(requestID, historicalDataRequests[requestID]);

                    Log(LogLevel.Info,
                        $"IB Repeating historical data request for {symbol} @ {historicalDataRequests[requestID].Frequency} with ID {requestID}");
                }
            }
        }

        public void Dispose()
        {
            client?.Dispose();

            if (requestRepeatTimer != null)
            {
                requestRepeatTimer.Dispose();
                requestRepeatTimer = null;
            }

            if (connectionStatusUpdateTimer != null)
            {
                connectionStatusUpdateTimer.Dispose();
                connectionStatusUpdateTimer = null;
            }
        }

        /// <summary>
        ///     Raise the event in a threadsafe manner
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <typeparam name="T"></typeparam>
        private static void RaiseEvent<T>(EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            var handler = @event;
            handler?.Invoke(sender, e);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}