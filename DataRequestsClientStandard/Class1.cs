using Common;
using Common.Utils;
using LZ4;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Interfaces;
using Common.Requests;

namespace DataRequestClient
{
    public class DataRequestClient : IDataClient
    {
        private const int HistoricalDataRequestsPeriodInSeconds = 1;
        private const int HeartBeatPeriodInSeconds = 1;

        /// <summary>
        ///     Initialization constructor.
        /// </summary>
        /// <param name="clientName">The name of this client. Should be unique. Used to route historical data.</param>
        /// <param name="host">The address of the server.</param>
        /// <param name="realTimeRequestPort">The port used for real time data requests.</param>
        /// <param name="realTimePublishPort">The port used for publishing new real time data.</param>
        /// <param name="historicalDataPort">The port used for historical data.</param>
        public DataRequestClient(string clientName, string host, int realTimeRequestPort, int realTimePublishPort,
            int historicalDataPort)
        {
            name = clientName;
            if (realTimeRequestPort == realTimePublishPort)
            {
                throw new Exception($"RealTimeRequestPort and ReamTimePublishPort can't be equal.");
            }

            realTimeRequestConnectionString = $"tcp://{host}:{realTimeRequestPort}";
            realTimeDataConnectionString = $"tcp://{host}:{realTimePublishPort}";
            historicalDataConnectionString = $"tcp://{host}:{historicalDataPort}";

            historicalDataRequests = new ConcurrentQueue<HistoricalDataRequest>();
        }

        private bool PollerRunning => poller != null && poller.IsRunning;

        /// <summary>
        ///     Keeps track of historical requests that have been sent but the data has not been received yet.
        /// </summary>
        public ObservableCollection<HistoricalDataRequest> PendingHistoricalRequests { get; } =
            new ObservableCollection<HistoricalDataRequest>();

        /// <summary>
        ///     Keeps track of live real time data streams.
        /// </summary>
        public ObservableCollection<RealTimeDataRequest> RealTimeDataStreams { get; } =
            new ObservableCollection<RealTimeDataRequest>();

        #region IDataClient Members

        public bool ClientRunningAndIsConnected => PollerRunning && (DateTime.Now - lastHeartBeat).TotalSeconds < 5;

        #region IDisposable implementation

        public void Dispose()
        {
           Disconnect();
        }

        #endregion IDisposable implementation

        #endregion IDataClient Members

        /// <summary>
        ///     Called when we get some sort of error reply
        /// </summary>
        private void HandleErrorReply()
        {
            // The request ID
            byte[] buffer = historicalDataSocket.ReceiveFrameBytes(out bool hasMore);
            if (!hasMore) return;
            int requestId = BitConverter.ToInt32(buffer, 0);
            // Remove from pending requests
            lock (pendingHistoricalRequestsLock)
            {
                PendingHistoricalRequests.RemoveAll(x => x.RequestID == requestId);
            }
            // Finally the error message
            string message = historicalDataSocket.ReceiveFrameString();
            // Raise the error event
            RaiseEvent(Error, this, new ErrorArgs(-1, message, requestId));
        }

        /// <summary>
        ///     Called when we get a reply on a request for available data in local storage.
        /// </summary>
        private void HandleAvailabledataReply()
        {
            // First the instrument
            byte[] buffer = historicalDataSocket.ReceiveFrameBytes();
            Instrument instrument;
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                instrument = MyUtils.ProtoBufDeserialize<Instrument>(ms);
            }
            // Second the number of items
            buffer = historicalDataSocket.ReceiveFrameBytes();
            int count = BitConverter.ToInt32(buffer, 0);
            // Then actually get the items, if any
            if (count == 0)
                RaiseEvent(LocallyAvailableDataInfoReceived, this,
                    new LocallyAvailableDataInfoReceivedEventArgs(instrument, new List<StoredDataInfo>()));
            else
            {
                List<StoredDataInfo> storageInfo = new List<StoredDataInfo>();

                for (int i = 0; i < count; i++)
                {
                    buffer = historicalDataSocket.ReceiveFrameBytes(out bool hasMore);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        StoredDataInfo info = MyUtils.ProtoBufDeserialize<StoredDataInfo>(ms);

                        storageInfo.Add(info);
                    }

                    if (!hasMore) break;
                }

                RaiseEvent(LocallyAvailableDataInfoReceived, this,
                    new LocallyAvailableDataInfoReceivedEventArgs(instrument, storageInfo));
            }
        }

        /// <summary>
        ///     Called on a reply to a data push
        /// </summary>
        private void HandleDataPushReply()
        {
            DataRequestMessageType result = (DataRequestMessageType)BitConverter.ToInt16(historicalDataSocket.ReceiveFrameBytes(), 0);

            switch (result)
            {
                case DataRequestMessageType.Success:
                    break;

                case DataRequestMessageType.Error:
                    // Receive the error
                    string error = historicalDataSocket.ReceiveFrameString();

                    RaiseEvent(Error, this, new ErrorArgs(-1, "Data push error: " + error));
                    break;
            }
        }

        /// <summary>
        ///     Called ona reply to a historical data request
        /// </summary>
        private void HandleHistoricalDataRequestReply()
        {
            byte[] requestBuffer = historicalDataSocket.ReceiveFrameBytes(out bool hasMore);

            // 2nd message part: the HistoricalDataRequest object that was used to make the request

            if (!hasMore) return;
            HistoricalDataRequest request;
            using (MemoryStream ms = new MemoryStream(requestBuffer))
            {
                request = MyUtils.ProtoBufDeserialize<HistoricalDataRequest>(ms);
            }
            // 3rd message part: the size of the uncompressed, serialized data. Necessary for decompression.
            byte[] sizeBuffer = historicalDataSocket.ReceiveFrameBytes(out hasMore);
            if (!hasMore) return;

            int outputSize = BitConverter.ToInt32(sizeBuffer, 0);
            // 4th message part: the compressed serialized data.
            byte[] dataBuffer = historicalDataSocket.ReceiveFrameBytes();
            byte[] decompressed = LZ4Codec.Decode(dataBuffer, 0, dataBuffer.Length, outputSize);
            List<OHLCBar> data;
            using (MemoryStream ms = new MemoryStream(decompressed))
            {
                data = MyUtils.ProtoBufDeserialize<List<OHLCBar>>(ms);
            }
            // Remove from pending requests
            lock (pendingHistoricalRequestsLock)
            {
                PendingHistoricalRequests.RemoveAll(x => x.RequestID == request.RequestID);
            }

            RaiseEvent(HistoricalDataReceived, this, new HistoricalDataEventArgs(request, data));
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
            EventHandler<T> handler = @event;
            handler?.Invoke(sender, e);
        }

        #region Variables

        // Where to connect
        private readonly string realTimeRequestConnectionString;

        private readonly string realTimeDataConnectionString;
        private readonly string historicalDataConnectionString;

        /// <summary>
        ///     This holds the zeromq identity string that we'll be using.
        /// </summary>
        private readonly string name;

        /// <summary>
        ///     Queue of historical data requests waiting to be sent out.
        /// </summary>
        private readonly ConcurrentQueue<HistoricalDataRequest> historicalDataRequests;

        private readonly object realTimeRequestSocketLock = new object();
        private readonly object realTimeDataSocketLock = new object();
        private readonly object historicalDataSocketLock = new object();
        private readonly object pendingHistoricalRequestsLock = new object();
        private readonly object realTimeDataStreamsLock = new object();

        /// <summary>
        ///     This socket sends requests for real time data.
        /// </summary>
        private DealerSocket realTimeRequestSocket;

        /// <summary>
        ///     This socket receives real time data.
        /// </summary>
        private SubscriberSocket realTimeDataSocket;

        /// <summary>
        ///     This socket sends requests for and receives historical data.
        /// </summary>
        private DealerSocket historicalDataSocket;

        /// <summary>
        ///     Pooler class to manage all used sockets.
        /// </summary>
        private NetMQPoller poller;

        /// <summary>
        ///     Periodically sends out requests for historical data and receives data when requests are fulfilled.
        /// </summary>
        private NetMQTimer historicalDataTimer;

        /// <summary>
        ///     Periodically sends heartbeat messages to server to ensure the connection is up.
        /// </summary>
        private NetMQTimer heartBeatTimer;

        /// <summary>
        ///     The time when the last heartbeat was received. If it's too long ago we're disconnected.
        /// </summary>
        private DateTime lastHeartBeat;

        /// <summary>
        ///     This int is used to give each historical request a unique RequestID.
        ///     Keep in mind this is unique to the CLIENT. AssignedID is unique to the server.
        /// </summary>
        private int requestCount;

        #endregion Variables

        #region IDataClient implementation

        public event EventHandler<RealTimeDataEventArgs> RealTimeDataReceived;

        public event EventHandler<HistoricalDataEventArgs> HistoricalDataReceived;

        public event EventHandler<LocallyAvailableDataInfoReceivedEventArgs> LocallyAvailableDataInfoReceived;

        public event EventHandler<ErrorArgs> Error;

        /// <summary>
        ///     Pushes data to local storage.
        /// </summary>
        public void PushData(DataAdditionRequest request)
        {
            if (request == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Request cannot be null."));

                return;
            }

            if (request.Instrument?.ID == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Instrument must be set and have an ID."));

                return;
            }
            if (!ClientRunningAndIsConnected)
            {
                RaiseEvent(Error, this,
                    new ErrorArgs(-1, "Could not push historical data to local storage - not connected."));

                return;
            }

            lock (historicalDataSocketLock)
            {
                if (historicalDataSocket != null)
                {
                    historicalDataSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.HistPush));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        historicalDataSocket.SendFrame(MyUtils.ProtoBufSerialize(request, ms));
                    }
                }
            }
        }

        /// <summary>
        ///     Requests information on what historical data is available in local storage for this instrument.
        /// </summary>
        public void GetLocallyAvailableDataInfo(Instrument instrument)
        {
            if (instrument == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Instrument cannot be null."));

                return;
            }

            if (!ClientRunningAndIsConnected)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not request local historical data - not connected."));

                return;
            }

            lock (historicalDataSocketLock)
            {
                if (historicalDataSocket != null)
                {
                    historicalDataSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.AvailableDataRequest));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        historicalDataSocket.SendFrame(MyUtils.ProtoBufSerialize(instrument, ms));
                    }
                }
            }
        }

        /// <summary>
        ///     Request historical data. Data will be delivered through the HistoricalDataReceived event.
        /// </summary>
        /// <returns>An ID uniquely identifying this historical data request. -1 if there was an error.</returns>
        public int RequestHistoricalData(HistoricalDataRequest request)
        {
            if (request == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Historical Data Request Failed: Request cannot be null."));

                return -1;
            }

            if (request.EndingDate < request.StartingDate)
            {
                RaiseEvent(Error, this,
                    new ErrorArgs(-1, "Historical Data Request Failed: Starting date must be after ending date."));

                return -1;
            }

            if (request.Instrument == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Historical Data Request Failed: Instrument cannot be null."));

                return -1;
            }

            if (!ClientRunningAndIsConnected)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not request historical data - not connected."));

                return -1;
            }

            if (!request.RTHOnly && request.Frequency >= BarSize.OneDay &&
                request.DataLocation != DataLocation.ExternalOnly)
            {
                RaiseEvent(
                    Error,
                    this,
                    new ErrorArgs(
                        -1,
                        "Warning: Requesting low-frequency data outside RTH should be done with DataLocation = ExternalOnly, data from local storage will be incorrect."));
            }

            request.RequestID = Interlocked.Increment(ref requestCount);

            lock (pendingHistoricalRequestsLock)
            {
                PendingHistoricalRequests.Add(request);
            }

            historicalDataRequests.Enqueue(request);

            return request.RequestID;
        }

        /// <summary>
        ///     Request a new real time data stream. Data will be delivered through the RealTimeDataReceived event.
        /// </summary>
        /// <returns>An ID uniquely identifying this real time data request. -1 if there was an error.</returns>
        public int RequestRealTimeData(RealTimeDataRequest request)
        {
            if (request == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Real Time Data Request Failed: Request cannot be null."));

                return -1;
            }

            if (request.Instrument == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Real Time Data Request Failed: null Instrument."));

                return -1;
            }

            if (!ClientRunningAndIsConnected)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not request real time data - not connected."));

                return -1;
            }

            request.RequestID = Interlocked.Increment(ref requestCount);

            lock (realTimeRequestSocketLock)
            {
                if (realTimeRequestSocket != null)
                {
                    // Two part message:
                    // 1: "RTD"
                    // 2: serialized RealTimeDataRequest
                    realTimeRequestSocket.SendMoreFrame(string.Empty);
                    realTimeRequestSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.RTDRequest));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        realTimeRequestSocket.SendFrame(MyUtils.ProtoBufSerialize(request, ms));
                    }
                }
            }

            return request.RequestID;
        }

        /// <summary>
        ///     Tries to connect to the QDMS server.
        /// </summary>
        public void Connect()
        {
            if (ClientRunningAndIsConnected)
                return;

            lock (realTimeRequestSocketLock)
            {
                if (CreateRealTimeRequestSocket()) return;
            }

            lock (realTimeDataSocketLock)
            {
                realTimeDataSocket = new SubscriberSocket(realTimeDataConnectionString);
                realTimeDataSocket.Options.Identity = Encoding.UTF8.GetBytes(name);
                realTimeDataSocket.ReceiveReady += RealTimeDataSocketReceiveReady;
            }

            lock (historicalDataSocketLock)
            {
                historicalDataSocket = new DealerSocket(historicalDataConnectionString);
                historicalDataSocket.Options.Identity = Encoding.UTF8.GetBytes(name);
                historicalDataSocket.ReceiveReady += HistoricalDataSocketReceiveReady;
            }

            lastHeartBeat = DateTime.Now;

            heartBeatTimer = new NetMQTimer(TimeSpan.FromSeconds(HeartBeatPeriodInSeconds));
            heartBeatTimer.Elapsed += HeartBeatTimerElapsed;

            historicalDataTimer = new NetMQTimer(TimeSpan.FromSeconds(HistoricalDataRequestsPeriodInSeconds));
            historicalDataTimer.Elapsed += HistoricalDataTimerElapsed;

            poller = new NetMQPoller
            {
                realTimeRequestSocket,
                realTimeDataSocket,
                historicalDataSocket,
                heartBeatTimer,
                historicalDataTimer
            };

            poller.RunAsync();
        }

        private bool CreateRealTimeRequestSocket()
        {
            realTimeRequestSocket = new DealerSocket(realTimeRequestConnectionString);
            realTimeRequestSocket.Options.Identity = Encoding.UTF8.GetBytes(name);
            // Start off by sending a ping to make sure everything is regular
            byte[] reply = new byte[] { };
            try
            {
                realTimeRequestSocket.SendMoreFrame(string.Empty)
                    .SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Ping));

                if (realTimeRequestSocket.TryReceiveFrameBytes(TimeSpan.FromSeconds(1), out reply))
                {
                    realTimeRequestSocket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(50), out reply);
                }
            }
            catch
            {
               Disconnect();
            }

            if (reply?.Length > 0)
            {
                DataRequestMessageType type = (DataRequestMessageType)BitConverter.ToInt16(reply, 0);

                if (type != DataRequestMessageType.Pong)
                {
                    try
                    {
                        realTimeRequestSocket.Disconnect(realTimeRequestConnectionString);
                    }
                    finally
                    {
                        realTimeRequestSocket.Close();
                        realTimeRequestSocket = null;
                    }

                    RaiseEvent(Error, this, new ErrorArgs(-1, "Could not connect to server."));

                    return true;
                }
            }
            else { RaiseEvent(Error, this, new ErrorArgs(-1, "Could not connect to server.")); }

            realTimeRequestSocket.ReceiveReady += RealTimeRequestSocketReceiveReady;
            return false;
        }

        /// <summary>
        ///     Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (!PollerRunning)
                return;
           
                while (RealTimeDataStreams.Count > 0)
                {
                    CancelRealTimeData(RealTimeDataStreams.First().Instrument);
                }
            

            poller?.Stop();
            poller?.Dispose();

            StopSockets();

            poller = null;
        }

        private void StopSockets()
        {
            StopRealTimeRequestSocket();

            StopRealTimeDataSocket();

            StopHistoricalDataSocket();
        }

        private void StopHistoricalDataSocket()
        {
            lock (historicalDataSocket)
            {
                if (historicalDataSocket != null)
                {
                    try
                    {
                        historicalDataSocket.Disconnect(historicalDataConnectionString);
                    }
                    finally
                    {
                        historicalDataSocket.ReceiveReady -= HistoricalDataSocketReceiveReady;
                        historicalDataSocket.Close();
                        historicalDataSocket = null;
                    }
                }
            }
        }

        private void StopRealTimeDataSocket()
        {
            lock (realTimeDataSocketLock)
            {
                if (realTimeDataSocket != null)
                {
                    try
                    {
                        realTimeDataSocket.Disconnect(realTimeDataConnectionString);
                    }
                    finally
                    {
                        realTimeDataSocket.ReceiveReady -= RealTimeDataSocketReceiveReady;
                        realTimeDataSocket.Close();
                        realTimeDataSocket = null;
                    }
                }
            }
        }

        private void StopRealTimeRequestSocket()
        {
            lock (realTimeRequestSocketLock)
            {
                if (realTimeRequestSocket != null)
                {
                    try
                    {
                        realTimeRequestSocket.Disconnect(realTimeRequestConnectionString);
                    }
                    finally
                    {
                        realTimeRequestSocket.ReceiveReady -= RealTimeRequestSocketReceiveReady;
                        realTimeRequestSocket.Close();
                        realTimeRequestSocket = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Cancel a live real time data stream.
        /// </summary>
        public void CancelRealTimeData(Instrument instrument)
        {
            if (instrument == null)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Instrument cannot be null."));

                return;
            }

            if (!ClientRunningAndIsConnected)
            {
                RaiseEvent(Error, this, new ErrorArgs(-1, "Could not cancel real time data - not connected."));

                return;
            }

            lock (realTimeRequestSocketLock)
            {
                if (realTimeRequestSocket != null)
                {
                    // Two part message:
                    // 1: "CANCEL"
                    // 2: serialized Instrument object
                    realTimeRequestSocket.SendMoreFrame(string.Empty);
                    realTimeRequestSocket.SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.CancelRTD));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        realTimeRequestSocket.SendFrame(MyUtils.ProtoBufSerialize(instrument, ms));
                    }
                }
            }

            lock (realTimeDataSocketLock)
            {
                realTimeDataSocket?.Unsubscribe(Encoding.UTF8.GetBytes(instrument.Symbol));
            }

            lock (realTimeDataStreamsLock)
            {
                RealTimeDataStreams.RemoveAll(x => x.Instrument.ID == instrument.ID);
            }
        }

        #endregion IDataClient implementation

        #region Event handlers

        /// <summary>
        ///     Process replies on the request socket.
        ///     Heartbeats, errors, and subscribing to real time data streams.
        /// </summary>
        private void RealTimeRequestSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            lock (realTimeRequestSocketLock)
            {
                string reply = realTimeRequestSocket?.ReceiveFrameString();

                if (reply == null)
                    return;

                DataRequestMessageType mesageTye = (DataRequestMessageType)BitConverter.ToInt16(realTimeRequestSocket.ReceiveFrameBytes(), 0);
                RealTimeDataRequest request;
                switch (mesageTye)
                {
                    case DataRequestMessageType.Pong:
                        lastHeartBeat = DateTime.Now;
                        break;

                    case DataRequestMessageType.Error:
                       
                        string error = realTimeRequestSocket.ReceiveFrameString();
                        byte[] buffer1 = realTimeRequestSocket.ReceiveFrameBytes();

                        using (MemoryStream ms = new MemoryStream(buffer1))
                        {
                            request = MyUtils.ProtoBufDeserialize<RealTimeDataRequest>(ms);
                        }
                        // Error event
                        RaiseEvent(Error, this,
                            new ErrorArgs(-1, "Real time data request error: " + error, request.RequestID));
                        break;

                    case DataRequestMessageType.Success:
                       
                        byte[] buffer = realTimeRequestSocket.ReceiveFrameBytes();
                        using (MemoryStream ms = new MemoryStream(buffer))
                        {
                            request = MyUtils.ProtoBufDeserialize<RealTimeDataRequest>(ms);
                        }
                       
                        lock (realTimeDataStreamsLock)
                        {
                            RealTimeDataStreams.Add(request);
                        }
                       
                        realTimeDataSocket.Subscribe(BitConverter.GetBytes(request.Instrument.ID));
                        break;

                    case DataRequestMessageType.RTDCanceled:
                        
                        string symbol = realTimeRequestSocket.ReceiveFrameString();
                        
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void RealTimeDataSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            lock (realTimeDataSocketLock)
            {
                if (realTimeDataSocket == null)
                    return;

                realTimeDataSocket.ReceiveFrameBytes(out bool hasMore);

                if (hasMore)
                {
                    byte[] buffer = realTimeDataSocket.ReceiveFrameBytes();

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        RealTimeDataEventArgs bar = MyUtils.ProtoBufDeserialize<RealTimeDataEventArgs>(ms);

                        RaiseEvent(RealTimeDataReceived, null, bar);
                    }
                }
            }
        }

        /// <summary>
        ///     Handling replies to a data push, a historical data request, or an available data request
        /// </summary>
        private void HistoricalDataSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            lock (historicalDataSocketLock)
            {
                if (historicalDataSocket == null)
                    return;
                // 1st message part: what kind of stuff we're receiving
                DataRequestMessageType type = (DataRequestMessageType)BitConverter.ToInt16(historicalDataSocket.ReceiveFrameBytes(), 0);
                switch (type)
                {
                    case DataRequestMessageType.Error:
                        HandleErrorReply();
                        break;

                    case DataRequestMessageType.AvailableDataReply:
                        HandleAvailabledataReply();
                        break;

                    case DataRequestMessageType.HistReply:
                        HandleHistoricalDataRequestReply();
                        break;

                    case DataRequestMessageType.HistPushReply:
                        HandleDataPushReply();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion Event handlers

        #region Timer handlers

        /// <summary>
        ///     Sends heartbeat messages so we know that the server is still up.
        /// </summary>
        private void HeartBeatTimerElapsed(object sender, NetMQTimerEventArgs e)
        {
            lock (realTimeRequestSocketLock)
            {
                if (PollerRunning && realTimeRequestSocket != null)
                {
                    realTimeRequestSocket.SendMoreFrame(string.Empty);
                    realTimeRequestSocket.SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Ping));
                }
            }
        }

        /// <summary>
        ///     Sends out requests for historical data and raises an event when it's received.
        /// </summary>
        private void HistoricalDataTimerElapsed(object sender, NetMQTimerEventArgs e)
        {
            if (!ClientRunningAndIsConnected)
                return;
            // TODO: Solve issue with _poller and socket in Disconnect method and here
            while (!historicalDataRequests.IsEmpty)
            {
                if (historicalDataRequests.TryDequeue(out HistoricalDataRequest request))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = MyUtils.ProtoBufSerialize(request, ms);

                        lock (historicalDataSocketLock)
                        {
                            if (PollerRunning && historicalDataSocket != null)
                            {
                                historicalDataSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.HistRequest));
                                historicalDataSocket.SendFrame(buffer);
                            }
                            else
                                return;
                        }
                    }
                }
            }
        }

        #endregion Timer handlers
    }
}