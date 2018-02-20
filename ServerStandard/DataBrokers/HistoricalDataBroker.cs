// The HistoricalDataBroker sits between the HistoricalDataServer
// and the external data source adapters.
// Requests for data are handled in RequestHistoricalData(),
// then forwarded to local storage, the appropriate external data source, or both.
// When data returns, it's sent through the HistoricalDataArrived event.

using Common;
using Common.Collections;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Interfaces;
using Common.Requests;
using Common.Utils;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using LogLevel = NLog.LogLevel;
using LogManager = NLog.LogManager;

namespace Server.DataBrokers
{
    public class HistoricalDataBroker : IHistoricalDataBroker
    {
        private readonly IDataStorage dataStorage;

        private readonly object localStorageLock = new object();

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly ConcurrentDictionary<int, HistoricalDataRequest> originalRequests;

        private readonly ConcurrentDictionary<int, List<HistoricalDataRequest>> subRequests;

        /// <summary>
        ///     Keeps track of IDs assigned to requests that have already been used, so there are no duplicates.
        /// </summary>
        private readonly List<int> usedIDs;

        private Timer connectionTimer;

        public HistoricalDataBroker(IDataStorage localStorage,
            IEnumerable<IHistoricalDataSource> additionalSources = null)
        {
            if (localStorage == null)
                throw new ArgumentNullException(nameof(localStorage));

            dataStorage = localStorage;

            //add the continuous futures broker to the data sources

            //add additional sources
            if (additionalSources != null)
            {
                foreach (IHistoricalDataSource ds in additionalSources)
                {
                    if (!DataSources.ContainsKey(ds.Name))
                    {
                        ds.Error += DatasourceError;
                        ds.HistoricalDataArrived += ExternalHistoricalDataArrived;
                        ds.Disconnected += SourceDisconnects;
                        DataSources.Add(ds.Name, ds);
                    }
                }
            }

            dataStorage.Error += DatasourceError;
            dataStorage.HistoricalDataArrived += LocalStorageHistoricalDataArrived;

            connectionTimer = new Timer(10000);
            connectionTimer.Elapsed += ConnectionTimerElapsed;
            connectionTimer.Start();

            originalRequests = new ConcurrentDictionary<int, HistoricalDataRequest>();
            subRequests = new ConcurrentDictionary<int, List<HistoricalDataRequest>>();
            usedIDs = new List<int>();

            TryConnect();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (connectionTimer != null)
            {
                connectionTimer.Dispose();
                connectionTimer = null;
            }

            foreach (var ds in DataSources.Values)
            {
                ds.Dispose();
            }
        }

        #endregion IDisposable Members

        #region IHistoricalDataBroker Members

        /// <summary>
        ///     Holds the real time data sources.
        /// </summary>
        public ObservableDictionary<string, IHistoricalDataSource> DataSources { get; } =
            new ObservableDictionary<string, IHistoricalDataSource>();

        public event EventHandler<HistoricalDataEventArgs> HistoricalDataArrived;

        public event EventHandler<ErrorArgs> Error;

        /// <summary>
        ///     Processes incoming historical data requests.
        /// </summary>
        public void RequestHistoricalData(HistoricalDataRequest request)
        {
            
            request.AssignedID = GetUniqueRequestID();

            originalRequests.TryAdd(request.AssignedID, request);

            if (RequestDataFromLocalStorateOnly(request)) return;

            CheckDataSource(request);

            if (RequestDataFromExternalSourceOnly(request)) return;

            lock (localStorageLock)
            {
                var localDataInfo = dataStorage.GetStoredDataInfo(request.Instrument.ID, request.Frequency);

                if (CheckIfLocalStorageCanSatisfyThisRequest(request, localDataInfo)) return;

                SendNewHistoricalDataRequest(request, localDataInfo);
            }
        }

        private void SendNewHistoricalDataRequest(HistoricalDataRequest request, StoredDataInfo localDataInfo)
        {
            if (localDataInfo == null)
            {
                ForwardHistoricalRequest(request);
            }
            else
            {
                GenerateSubRequests(request, localDataInfo);
            }
        }

        private bool CheckIfLocalStorageCanSatisfyThisRequest(HistoricalDataRequest request, StoredDataInfo localDataInfo)
        {
            if (localDataInfo != null
                && localDataInfo.LatestDate >= request.EndingDate
                && localDataInfo.EarliestDate <= request.StartingDate)
            {
                dataStorage.RequestHistoricalData(request);
                return true;
            }

            if (localDataInfo != null)
            {
                int dotw = request.Instrument.Expiration.Date.DayOfWeek.ToInt();
                InstrumentSession session = request.Instrument.Sessions?.First(x => (int)x.ClosingDay == dotw && x.IsSessionEnd);

                if (session != null
                    && localDataInfo.LatestDate >= request.Instrument.Expiration.Date + session.ClosingTime)
                {
                    dataStorage.RequestHistoricalData(request);
                    return true;
                }
            }
            return false;
        }

        private void CheckDataSource(HistoricalDataRequest request)
        {
            try
            {
                CheckDataSource(request.Instrument.Datasource.Name);
            }
            catch (Exception ex)
            {
                ForwardHistoricalRequest(request);
                Log(LogLevel.Error,
                    $"Data source problem for request ID {request.AssignedID}, forwarded directly to local storage. Error: {ex.Message}");
                throw;
            }
        }

        private bool RequestDataFromLocalStorateOnly(HistoricalDataRequest request)
        {
            if (request.DataLocation == DataLocation.LocalOnly)
            {
                lock (localStorageLock)
                {
                    dataStorage.RequestHistoricalData(request);
                }
                return true;
            }
            return false;
        }

        private bool RequestDataFromExternalSourceOnly(HistoricalDataRequest request)
        {
            if (request.DataLocation == DataLocation.ExternalOnly)
            {
                ForwardHistoricalRequest(request);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Forwards a data addition request to local storage.
        /// </summary>
        public void AddData(DataAdditionRequest request)
        {
            if (request.Data.Count == 0)
            {
                Log(LogLevel.Info, $"HDB: AddData called with zero bars, request: {request}");
                return;
            }

            lock (localStorageLock)
            {
                dataStorage.AddData(request.Data, request.Instrument, request.Frequency, request.Overwrite);
            }
        }

        public List<StoredDataInfo> GetAvailableDataInfo(Instrument instrument)
        {
            lock (localStorageLock)
            {
                return dataStorage.GetStoredDataInfo(instrument.ID);
            }
        }

        #endregion IHistoricalDataBroker Members

        /// <summary>
        ///     This method is called when a data source disconnects
        /// </summary>
        private void SourceDisconnects(object sender, DataSourceDisconnectEventArgs e)
        {
            Log(LogLevel.Info, $"Historical Data Broker: Data source {e.SourceName} disconnected");
        }

        /// <summary>
        ///     Add a message to the log.
        /// </summary>
        private void Log(LogLevel level, string message)
        {
            logger.Log(level, message);
        }

        /// <summary>
        ///     This method handles data arrivals from the local database
        /// </summary>
        private void LocalStorageHistoricalDataArrived(object sender, HistoricalDataEventArgs e)
        {
            Log(LogLevel.Info,
                $"Pulled {e.Data.Count} data points from local storage on instrument {e.Request.Instrument.Symbol}.");

            //pass up the data to the server so it can be sent out
            ReturnData(e);
        }

        /// <summary>
        ///     Raise the event in a thread safe manner
        /// </summary>
        private static void RaiseEvent<T>(EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            var handler = @event;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        ///     This one handles data arrivals from historical data sources other than local storage
        /// </summary>
        private void ExternalHistoricalDataArrived(object sender, HistoricalDataEventArgs e)
        {
            var assignedID = e.Request.IsSubrequestFor ?? e.Request.AssignedID;
            var gotOriginalRequest = originalRequests.TryGetValue(assignedID, out HistoricalDataRequest originalRequest);
            if (!gotOriginalRequest)
            {
                throw new Exception("Something went wrong: original request disappeared");
            }

            if (e.Request.SaveToLocalStorage)
            {
                SaveToLocaLStorage(e, originalRequest);
            }
            else
            {
                var storageData = new List<OHLCBar>();
                if (e.Data.Count > 0 &&
                    e.Data[0].Date().ToDateTime() > originalRequest.StartingDate &&
                    e.Request.DataLocation != DataLocation.ExternalOnly)
                {
                    lock (localStorageLock)
                    {
                        //we add half a bar to the request limit so that the data we get starts with the next one
                        DateTime correctedDateTime =
                            e.Data[0].Date().Date.ToDateTime()
                                .AddMilliseconds(originalRequest.Frequency.ToTimeSpan().TotalMilliseconds / 2);
                        storageData = dataStorage.GetData(originalRequest.Instrument, originalRequest.StartingDate,
                            correctedDateTime, originalRequest.Frequency);
                    }
                }

                ReturnData(new HistoricalDataEventArgs(e.Request, storageData.Concat(e.Data).ToList()));

                Log(LogLevel.Info,
                    $"Pulled {e.Data.Count} data points from source {e.Request.Instrument.Datasource.Name} on instrument {e.Request.Instrument.Symbol} and {storageData.Count} points from local storage.");
            }
        }

        private void SaveToLocaLStorage(HistoricalDataEventArgs e, HistoricalDataRequest originalRequest)
        {
            HistoricalDataRequest historicalDataRequest = e.Request;
            AddData(new DataAdditionRequest(historicalDataRequest.Frequency, historicalDataRequest.Instrument, e.Data));

            if (historicalDataRequest.IsSubrequestFor.HasValue && subRequests.ContainsKey(historicalDataRequest.IsSubrequestFor.Value))
            {
                subRequests[historicalDataRequest.IsSubrequestFor.Value].Remove(historicalDataRequest);
                if (subRequests[historicalDataRequest.IsSubrequestFor.Value].Count == 0)
                {
                    subRequests.TryRemove(historicalDataRequest.IsSubrequestFor.Value, out List<HistoricalDataRequest> tmpList);

                    lock (localStorageLock)
                    {
                        dataStorage.RequestHistoricalData(originalRequest);
                    }
                }
            }
            else
            {
                if (historicalDataRequest.DataLocation == DataLocation.ExternalOnly)
                {
                    //if the request specifies only fresh data, we don't want to go through local storage
                    ReturnData(new HistoricalDataEventArgs(historicalDataRequest, e.Data));
                }
                else
                    lock (localStorageLock)
                    {
                        dataStorage.RequestHistoricalData(originalRequest);
                    }
            }

            Log(LogLevel.Info,
                $"Pulled {e.Data.Count} data points from source {historicalDataRequest.Instrument.Datasource.Name} on instrument {historicalDataRequest.Instrument.Symbol}.");
        }

        /// <summary>
        ///     Raise the event that returns data to the server, after applying an RTH filter if needed.
        /// </summary>
        private void ReturnData(HistoricalDataEventArgs e)
        {
            //if needed, we filter out the data outside of regular trading hours
            if (e.Request.RTHOnly &&
                e.Request.Frequency < BarSize.OneDay &&
                e.Request.Instrument.Sessions != null)
            {
                RthFilter.Filter(e.Data, e.Request.Instrument.Sessions.ToList());
            }

            //For daily+ data using RTH, we set the bar opening and closing times using
            if (e.Request.Frequency >= BarSize.OneDay)
            {
                MyUtils.SetSessionTimes(e.Data, e.Request.Instrument);
            }

            RaiseEvent(HistoricalDataArrived, this, new HistoricalDataEventArgs(e.Request, e.Data));
        }

        /// <summary>
        ///     Fires when any of the underlying data sources raise their error event.
        /// </summary>
        private void DatasourceError(object sender, ErrorArgs e)
        {
            RaiseEvent(Error, sender, new ErrorArgs(-1, "HDB Client Error: " + e.ErrorMessage, e.RequestID));
            Log(LogLevel.Error, $"HDB: {e.ErrorCode} - {e.ErrorMessage}");
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
            if (!dataStorage.Connected)
                lock (localStorageLock)
                {
                    dataStorage.Connect();
                }

            foreach (KeyValuePair<string, IHistoricalDataSource> s in DataSources)
            {
                if (!s.Value.Connected)
                {
                    Log(LogLevel.Info, $"Historical Data Broker: Trying to connect to data source {s.Key}");
                    s.Value.Connect();
                }
            }
        }

        /// <summary>
        ///     Ensures that the data source specified is present and connected.
        ///     Throws an exception otherwise.
        /// </summary>
        private void CheckDataSource(string name)
        {
            if (!DataSources.ContainsKey(name))
                throw new Exception($"Data source {name} does not exist.");
            if (!DataSources[name].Connected)
                throw new Exception($"Data source {name} is not connected.");
        }

        /// <summary>
        ///     When a data request can be partly filled by the local db,
        ///     we need to split it into one or two sub-requests for the parts that
        ///     are not locally available. This method does that and forwards the sub-requests.
        /// </summary>
        private void GenerateSubRequests(HistoricalDataRequest request, StoredDataInfo localDataInfo)
        {
            subRequests.TryAdd(request.AssignedID, new List<HistoricalDataRequest>());

            HistoricalDataRequest newBackRequest = CheckIfEarlierDataMayBeNeeded(request, localDataInfo);

            HistoricalDataRequest newForwardRequest = CheckIfLaterDataMayBeNeeded(request, localDataInfo);

            //we send these together, because too large of a delay between the two requests can cause problems
            if (newBackRequest != null)
                ForwardHistoricalRequest(newBackRequest);
            if (newForwardRequest != null)
                ForwardHistoricalRequest(newForwardRequest);
        }

        private HistoricalDataRequest CheckIfEarlierDataMayBeNeeded(HistoricalDataRequest request, StoredDataInfo localDataInfo)
        {
            HistoricalDataRequest newBackRequest = null;
            if (localDataInfo.EarliestDate > request.StartingDate)
            {
                newBackRequest = (HistoricalDataRequest)request;
                newBackRequest.EndingDate =
                    localDataInfo.EarliestDate.AddMilliseconds(-request.Frequency.ToTimeSpan().TotalMilliseconds / 2);
                newBackRequest.IsSubrequestFor = request.AssignedID;
                newBackRequest.AssignedID = GetUniqueRequestID();
                subRequests[newBackRequest.IsSubrequestFor.Value].Add(newBackRequest);
            }
            return newBackRequest;
        }

        private HistoricalDataRequest CheckIfLaterDataMayBeNeeded(HistoricalDataRequest request, StoredDataInfo localDataInfo)
        {
            HistoricalDataRequest newForwardRequest = null;
            if (localDataInfo.LatestDate < request.EndingDate)
            {
                //the local storage is insufficient, so we save the original request, make a copy,
                //modify it, and pass it to the external data source
                newForwardRequest = (HistoricalDataRequest)request;
                newForwardRequest.StartingDate =
                    localDataInfo.LatestDate.AddMilliseconds(request.Frequency.ToTimeSpan().TotalMilliseconds / 2);
                newForwardRequest.IsSubrequestFor = request.AssignedID;
                newForwardRequest.AssignedID = GetUniqueRequestID();
                subRequests[newForwardRequest.IsSubrequestFor.Value].Add(newForwardRequest);
            }
            return newForwardRequest;
        }

        /// <summary>
        ///     Sends off a historical data request to the datasource that needs to fulfill it
        /// </summary>
        private void ForwardHistoricalRequest(HistoricalDataRequest request)
        {
            var timezone = request.Instrument.Exchange == null ? "UTC" : request.Instrument.Exchange.Timezone;
            var exchangeTz = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            //limit the ending date to the present
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, exchangeTz);
            var endDate = request.EndingDate > now ? now : request.EndingDate;
            request.EndingDate = endDate;
            request.StartingDate = TimeZoneInfo.ConvertTime(request.StartingDate, TimeZoneInfo.Local, exchangeTz);

            DataSources[request.Instrument.Datasource.Name].RequestHistoricalData(request);
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
    }
}