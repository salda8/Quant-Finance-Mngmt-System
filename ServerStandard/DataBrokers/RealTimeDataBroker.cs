// The RealTimeDataBroker sits between the HistoricalDataServer
// and the external data source adapters.
// Requests for new real time data streams are handled in RequestRealTimeData(),
// then forwarded the appropriate external data source (if the stream doesn't already exist)

using Common;
using Common.Collections;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Interfaces;
using Common.Requests;
using Common.Utils;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;
using LogLevel = NLog.LogLevel;

#if !DEBUG
using System.Net;
#endif

namespace Server.DataBrokers
{
    public class RealTimeDataBroker : IDisposable, IRealTimeDataBroker
    {
        private readonly object activeStreamsLock = new object();

        /// <summary>
        ///     When bars arrive, the data source raises an event
        ///     the event adds the data to the _arrivedBars
        ///     then the publishing server sends out the data
        /// </summary>
        private readonly BlockingCollection<RealTimeDataEventArgs> arrivedBars;

        private readonly IDataStorage localStorage;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Here we store the requests, key is the AssignedID.
        /// </summary>
        private readonly Dictionary<int, RealTimeDataRequest> requests;

        private readonly object requestsLock = new object();

        private readonly object subscriberCountLock = new object();

        /// <summary>
        ///     Keeps track of IDs assigned to requests that have already been used, so there are no duplicates.
        /// </summary>
        private readonly List<int> usedIDs;

        private Timer connectionTimer; //tries to reconnect every once in a while

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="localStorage"></param>
        /// <param name="additionalDataSources">Optional. Pass any additional data sources (for testing purposes).</param>
        public RealTimeDataBroker(IDataStorage localStorage,
            IEnumerable<IRealTimeDataSource> additionalDataSources = null)
        {
            connectionTimer = new Timer(10000);
            connectionTimer.Elapsed += ConnectionTimerElapsed;
            connectionTimer.Start();
            DataSources = new ObservableDictionary<string, IRealTimeDataSource>();

            if (additionalDataSources != null)
            {
                foreach (var ds in additionalDataSources)
                {
                    ds.DataReceived += RealTimeData;
                    ds.Disconnected += SourceDisconnects;
                    ds.Error += s_Error;
                    DataSources.Add(ds.Name, ds);
                }
            }

            //we need to set the appropriate event methods for every data source
            //foreach (var s in DataSources.Values)
            //{
            //    s.DataReceived += RealTimeData;
            //    s.Disconnected += SourceDisconnects;
            //    s.Error += s_Error;
            //}

            ActiveStreams = new ConcurrentNotifierBlockingList<RealTimeStreamInfo>();
            arrivedBars = new BlockingCollection<RealTimeDataEventArgs>();
            StreamSubscribersCount = new Dictionary<RealTimeStreamInfo, int>();
            requests = new Dictionary<int, RealTimeDataRequest>();
            usedIDs = new List<int>();

            //connect to our data sources
            TryConnect();

            //local storage
            this.localStorage = localStorage;
        }

        /// <summary>
        ///     Here we keep track of what clients are subscribed to what data streams.
        ///     KVP consists of key: request ID, value: data source name.
        ///     The int is simply the number of clients subscribed to that stream.
        /// </summary>
        private Dictionary<RealTimeStreamInfo, int> StreamSubscribersCount { get; }

        #region IDisposable Members

        public void Dispose()
        {
            if (connectionTimer != null)
            {
                connectionTimer.Dispose();
                connectionTimer = null;
            }

            arrivedBars?.Dispose();
        }

        #endregion IDisposable Members

        #region IRealTimeDataBroker Members

        /// <summary>
        ///     Holds the real time data sources.
        /// </summary>
        public ObservableDictionary<string, IRealTimeDataSource> DataSources { get; }

        /// <summary>
        ///     Holds the active data streams. They KVP consists of key: request ID, value: data source name
        /// </summary>
        public ConcurrentNotifierBlockingList<RealTimeStreamInfo> ActiveStreams { get; }

        /// <summary>
        ///     Request to initiate a real time data stream.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>True is the request was successful, false otherwise.</returns>
        public bool RequestRealTimeData(RealTimeDataRequest request)
        {
            request.AssignedID = GetUniqueRequestID();
            lock (requestsLock)
            {
                requests.Add(request.AssignedID, request);
            }

            //if there is already an active stream of this instrument
            bool streamExists;
            lock (activeStreamsLock)
            {
                streamExists = ActiveStreams.Collection.Any(x => x.Instrument.ID == request.Instrument.ID);
            }
            if (streamExists)
            {
                IncrementSubscriberCount(request.Instrument);

                //log the request
                Log(LogLevel.Info,
                    $"RTD Request for existing stream: {request.Instrument.Symbol} from {request.Instrument.Datasource.Name} @ {Enum.GetName(typeof(BarSize), request.Frequency)}");

                return true;
            }
            if (DataSources.ContainsKey(request.Instrument.Datasource.Name) &&
                //make sure the datasource is present & connected
                DataSources[request.Instrument.Datasource.Name].Connected)
            {
                ForwardRtdRequest(request);

                return true;
            }
            if (!DataSources.ContainsKey(request.Instrument.Datasource.Name))
                throw new Exception("No such datasource.");
            if (!DataSources[request.Instrument.Datasource.Name].Connected)
                throw new Exception("Datasource not connected.");
            return false;
        }

        /// <summary>
        ///     Cancel a real time data stream and clean up after it.
        /// </summary>
        /// <returns>True if the stream was canceled, False if subscribers remain.</returns>
        public bool CancelRtdStream(int instrumentID)
        {
            lock (activeStreamsLock)
            {
                //make sure there is a data stream for this instrument
                if (ActiveStreams.Collection.Any(x => x.Instrument.ID == instrumentID))
                {
                    var streamInfo = ActiveStreams.Collection.First(x => x.Instrument.ID == instrumentID);
                    var instrument = streamInfo.Instrument;

                    //log the request
                    Log(LogLevel.Info,
                        $"RTD Cancelation request: {instrument.Symbol} from {instrument.Datasource.Name}");

                    lock (subscriberCountLock)
                    {
                        StreamSubscribersCount[streamInfo]--;
                        if (StreamSubscribersCount[streamInfo] == 0)
                        {
                            //there are no clients subscribed to this stream anymore
                            //cancel it and remove it from all the places
                            StreamSubscribersCount.Remove(streamInfo);

                            ActiveStreams.TryRemove(streamInfo);
                            DataSources[streamInfo.Datasource].CancelRealTimeData(streamInfo.RequestID);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public event EventHandler<RealTimeDataEventArgs> RealTimeDataArrived;

        #endregion IRealTimeDataBroker Members

        /// <summary>
        ///     When one of the data sources has some sort of error, it raises an event which is handled by this method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void s_Error(object sender, ErrorArgs e)
        {
            Log(LogLevel.Error, $"RTB: {e.ErrorCode} - {e.ErrorMessage}");
        }

        /// <summary>
        ///     Gets a new unique AssignedID to identify requests with.
        /// </summary>
        private int GetUniqueRequestID()
        {
            //requests can arrive very close to each other and thus have the same seed, so we need to make sure it's unique
            var rand = new Random();
            int id;
            do
            {
                id = rand.Next(1, int.MaxValue);
            } while (usedIDs.Contains(id));

            usedIDs.Add(id);
            return id;
        }

        /// <summary>
        ///     When one of the data sources receives new real time data, it raises an event which is handled by this method,
        ///     which then forwards the data over the PUB socket after serializing it.
        /// </summary>
        public void RealTimeData(object sender, RealTimeDataEventArgs e)
        {
            RaiseEvent(RealTimeDataArrived, this, e);

            //save to local storage
            //perhaps just add it to a queue and then process in a different thread
            lock (requestsLock)
            {
                if (requests[e.RequestID].SaveToLocalStorage)
                {
                    localStorage.AddData(
                        new OHLCBar
                        {
                            Open = e.Open,
                            High = e.High,
                            Low = e.Low,
                            Close = e.Close,
                            Volume = e.Volume,
                            DateTimeClose = MyUtils.TimestampToDateTime(e.Time)
                        },
                        requests[e.RequestID].Instrument,
                        requests[e.RequestID].Frequency);
                }
            }

#if DEBUG
            Log(LogLevel.Trace,
                $"RTD Received Instrument ID: {e.InstrumentID} O:{e.Open} H:{e.High} L:{e.Low} C:{e.Close} V:{e.Volume} T:{e.Time}");
#endif
        }

        /// <summary>
        ///     Log stuff.
        /// </summary>
        private void Log(LogLevel level, string message)
        {
            logger.Log(level, message);
        }

        /// <summary>
        ///     Raise the event in a threadsafe manner
        /// </summary>
        private static void RaiseEvent<T>(EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            var handler = @event;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        ///     Increments the number of subscribers to a real time data stream by 1.
        /// </summary>
        private void IncrementSubscriberCount(Instrument instrument)
        {
            lock (activeStreamsLock)
            {
                //Find the KeyValuePair<string, int> from the dictionary that corresponds to this instrument
                //The KVP consists of key: request ID, value: data source name
                var streamInfo = ActiveStreams.Collection.First(x => x.Instrument.ID == instrument.ID);

                //increment the subscriber count
                lock (subscriberCountLock)
                {
                    StreamSubscribersCount[streamInfo]++;
                }
            }
        }

        /// <summary>
        ///     Sends a real time data request to the correct data source, logs it, and updates subscriber counts
        /// </summary>
        /// <param name="request"></param>
        private void ForwardRtdRequest(RealTimeDataRequest request)
        {
            //send the request to the correct data source
            int reqID;
            try
            {
                reqID = DataSources[request.Instrument.Datasource.Name].RequestRealTimeData(request);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Error requesting real time data: " + ex.Message);
                return;
            }

            //log the request
            Log(LogLevel.Info,
                $"RTD Request: {request.Instrument.Symbol} from {request.Instrument.Datasource.Name} @ {Enum.GetName(typeof(BarSize), request.Frequency)} ID:{reqID}");

            //add the request to the active streams, though it's not necessarily active yet
            var streamInfo = new RealTimeStreamInfo(
                request.Instrument,
                reqID,
                request.Instrument.Datasource.Name,
                request.Frequency,
                request.RTHOnly);

            lock (activeStreamsLock)
            {
                ActiveStreams.TryAdd(streamInfo);
            }

            lock (subscriberCountLock)
            {
                StreamSubscribersCount.Add(streamInfo, 1);
            }
        }

        /// <summary>
        ///     This method is called when a data source disconnects
        /// </summary>
        private void SourceDisconnects(object sender, DataSourceDisconnectEventArgs e)
        {
            Log(LogLevel.Info, $"Real Time Data Broker: Data source {e.SourceName} disconnected");
        }

        /// <summary>
        ///     There is a timer which periodically calls the tryconnect function to connect to any disconnected data sources
        /// </summary>
        private void ConnectionTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TryConnect();
        }

        /// <summary>
        ///     Loops through data sources and tries to connect to those that are disconnected
        /// </summary>
        private void TryConnect()
        {
            foreach (KeyValuePair<string, IRealTimeDataSource> s in DataSources)
            {
                if (!s.Value.Connected)
                {
                    Log(LogLevel.Info, $"Real Time Data Broker: Trying to connect to data source {s.Key}");

#if !DEBUG
                    try
                    {
#endif
                    s.Value.Connect();
#if !DEBUG
                    }
                    catch (WebException ex)
                    {
                        logger.Error(ex, "Real Time Data Broker: Error while connecting to data source {0}", s.Key);
                    }
#endif
                }
            }
        }
    }
}