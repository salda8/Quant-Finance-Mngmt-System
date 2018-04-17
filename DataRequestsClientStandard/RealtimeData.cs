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
    public class RealtimeData : IDataClient
    {
      
        private const int HeartBeatPeriodInSeconds = 1;

        /// <summary>
        ///     Initialization constructor.
        /// </summary>
        /// <param name="clientName">The name of this client. Should be unique. Used to route historical data.</param>
        /// <param name="host">The address of the server.</param>
        /// <param name="realTimeRequestPort">The port used for real time data requests.</param>
        /// <param name="realTimePublishPort">The port used for publishing new real time data.</param>
       public RealtimeData(string clientName, string host, int realTimeRequestPort, int realTimePublishPort
           )
        {
            name = clientName;
            if (realTimeRequestPort == realTimePublishPort)
            {
                throw new Exception($"RealTimeRequestPort and ReamTimePublishPort can't be equal.");
            }

            realTimeRequestConnectionString = $"tcp://{host}:{realTimeRequestPort}";
            realTimeDataConnectionString = $"tcp://{host}:{realTimePublishPort}";
           
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
       

        /// <summary>
        ///     This holds the zeromq identity string that we'll be using.
        /// </summary>
        private readonly string name;
        
        private readonly object realTimeRequestSocketLock = new object();
        private readonly object realTimeDataSocketLock = new object();
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




        public event EventHandler<ErrorArgs> Error;



       

        

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
            
            lastHeartBeat = DateTime.Now;

            heartBeatTimer = new NetMQTimer(TimeSpan.FromSeconds(HeartBeatPeriodInSeconds));
            heartBeatTimer.Elapsed += HeartBeatTimerElapsed;

           

            poller = new NetMQPoller
            {
                realTimeRequestSocket,
                realTimeDataSocket,
                historicalDataSocket,
                heartBeatTimer,
               
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
        

        #endregion Timer handlers
    }
}