using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.ExtensionMethods;
using Common.Requests;
using Common.Utils;
using LZ4;
using NetMQ;
using NetMQ.Sockets;

namespace DataRequestsClientStandard
{
    public class HistoricalData
    {
        private string historicalDataConnectionString;
        private ConcurrentQueue<HistoricalDataRequest> historicalDataRequests;
        private DateTime lastHeartBeat;
        private NetMQPoller poller;
        private object historicalDataSocketLock= new object();
        private NetMQTimer heartBeatTimer;
        private DealerSocket historicalDataSocket;
        private string name;
        private NetMQTimer historicalDataTimer;
        private object pendingHistoricalRequestsLock = new object();
        private int requestCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalData"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port"> The port used for historical data.</param>
        /// <param name="clientName">Name of the client.</param>
        public HistoricalData(string host, int port, string clientName)
        {
            name = clientName;
            historicalDataConnectionString = $"tcp://{host}:{port}";

            historicalDataRequests = new ConcurrentQueue<HistoricalDataRequest>();
        }

        public bool ClientRunningAndIsConnected => PollerRunning && (DateTime.Now - lastHeartBeat).TotalSeconds < 5;
        public bool PollerRunning { get; set; }
        /// <summary>
        ///     Tries to connect to the QDMS server.
        /// </summary>
        public void Connect()
        {
            if (ClientRunningAndIsConnected)
                return;
            
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

            lock (historicalDataSocketLock)
            {
                poller = new NetMQPoller
                {
                    historicalDataSocket,
                    heartBeatTimer,
                    historicalDataTimer
                };
            }

            poller.RunAsync();
        }

        public double HistoricalDataRequestsPeriodInSeconds { get; set; }
        

        private void HeartBeatTimerElapsed(object sender, NetMQTimerEventArgs e)
        {
            //lock (realTimeRequestSocketLock)
            //{
            //    if (PollerRunning && realTimeRequestSocket != null)
            //    {
            //        realTimeRequestSocket.SendMoreFrame(string.Empty);
            //        realTimeRequestSocket.SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Ping));
            //    }
            //}
        }

        public double HeartBeatPeriodInSeconds { get; set; }

        /// <summary>
        ///     Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (!PollerRunning)
                return;

            
            poller?.Stop();
            poller?.Dispose();

            StopHistoricalDataSocket();

            poller = null;
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

        public event EventHandler<ErrorArgs> Error;
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
        ///     Keeps track of historical requests that have been sent but the data has not been received yet.
        /// </summary>
        public ObservableCollection<HistoricalDataRequest> PendingHistoricalRequests { get; } =
            new ObservableCollection<HistoricalDataRequest>();
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
            {
                RaiseEvent(LocallyAvailableDataInfoReceived, this,
                    new LocallyAvailableDataInfoReceivedEventArgs(instrument, new List<StoredDataInfo>()));
            }
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


        public event EventHandler<LocallyAvailableDataInfoReceivedEventArgs> LocallyAvailableDataInfoReceived;
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
        public event EventHandler<HistoricalDataEventArgs> HistoricalDataReceived;
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
                            {
                                return;
                            }
                        }

                    }
                }
            }
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

    }
}
