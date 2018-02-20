// This class handles networking for real time data data.
// Clients send their requests through ZeroMQ. Here they are parsed
// and then forwarded to the RealTimeDataBroker.
// Data sent from the RealTimeDataBroker is sent out to the clients.
// Two types of possible requests:
// 1. To start receiving real time data
// 2. To cancel a real time data stream

using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Interfaces;
using Common.Requests;
using Common.Utils;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using ProtoBuf;
using System;
using System.IO;
using DataAccess.EntityConfigs;
using Server.Servers;
using Server.Utils;

namespace Servers
{
    public class RealTimeDataServer : INetMQServer
    {
        private readonly IRealTimeDataBroker broker;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string publisherConnectionString;
        private readonly object publisherSocketLock = new object();
        private readonly string requestConnectionString;
        private readonly object requestSocketLock = new object();
        private NetMQPoller poller;
        //todo not needede?
        private NetMQSocket publisherSocket;
        private NetMQSocket requestSocket;

        public RealTimeDataServer(int requestPort, IRealTimeDataBroker broker, string host = "localhost", string transportProtocol = "tcp") 
            : this(new MqServer(requestPort, host, (TransportProtocol)Enum.Parse(typeof(TransportProtocol),transportProtocol)), broker)
        {
           
        }

        public RealTimeDataServer(MqServer server, IRealTimeDataBroker broker) 
        {
            if (!ServerUtils.IsPortOpen(server.HostIp, server.Port, TimeSpan.FromSeconds(2)))
            {   
                throw new PortIsInUseException($"{server.GetAddress()} is in use.");
            }
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
            this.broker.RealTimeDataArrived += BrokerRealTimeDataArrived;
            //if (publish.GetAddress() == request.GetAddress())
            //{
            //    throw new ArgumentException("Publish and request ports must be different");
            //}

            //publisherConnectionString = publish.GetAddress();
            
            requestConnectionString = server.GetAddress();

        }

        /// <summary>
        ///     Whether the server is running or not.
        /// </summary>
        public bool ServerRunning => poller != null && poller.IsRunning;

        #region IDisposable Members

        #region IDisposable implementation

        public void Dispose()
        {
            StopServer();
        }

        #endregion IDisposable implementation

        #endregion IDisposable Members

        /// <summary>
        ///     Starts the server.
        /// </summary>
        public void StartServer()
        {
            if (ServerRunning)
                return;


           

            lock (requestSocketLock)
            {
                requestSocket = new ResponseSocket(requestConnectionString);
                requestSocket.ReceiveReady += RequestSocketReceiveReady;
                poller = new NetMQPoller { requestSocket };
                poller.RunAsync();
            }


           
        }

        /// <summary>
        ///     Stops the server.
        /// </summary>
        public void StopServer()
        {
            if (!ServerRunning)
                return;

            poller?.Stop();
            poller?.Dispose();

            lock (publisherSocketLock)
            {
                if (publisherSocket != null)
                {
                    try
                    {
                        publisherSocket.Disconnect(publisherConnectionString);
                    }
                    finally
                    {
                        publisherSocket.Close();
                        publisherSocket = null;
                    }
                }
            }

            lock (requestSocketLock)
            {
                if (requestSocket != null)
                {
                    try
                    {
                        requestSocket.Disconnect(requestConnectionString);
                    }
                    finally
                    {
                        requestSocket.ReceiveReady -= RequestSocketReceiveReady;
                        requestSocket.Close();
                        requestSocket = null;
                    }
                }
            }

            poller = null;
        }

        // Accept a real time data request
        private void HandleRealTimeDataRequest()
        {
            var buffer = requestSocket.ReceiveFrameBytes();
            using (var ms = new MemoryStream(buffer))
            {
                var request = MyUtils.ProtoBufDeserialize<RealTimeDataRequest>(ms);
                // Make sure the ID and data sources are set
                
                if (request.Instrument.Datasource == null)
                {
                    SendErrorReply("Instrument had no data source set.", buffer);

                    logger.Error("Instrument with no data source requested.");

                    return;
                }
                // With the current approach we can't handle multiple real time data streams from
                // the same symbol and data source, but at different frequencies

                // Forward the request to the broker
                try
                {
                    if (broker.RequestRealTimeData(request))
                    {
                        // And report success back to the requesting client
                        requestSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Success));
                        // Along with the request
                        requestSocket.SendFrame(MyUtils.ProtoBufSerialize(request, ms));
                    }
                    else
                        throw new Exception("Unknown error.");
                }
                catch (Exception ex)
                {
                    SendErrorReply(ex.Message, buffer);

                    logger.Error(
                        $"RTDS: Error handling RTD request {request.Instrument.Symbol} @ {request.Instrument.Datasource} ({request.Frequency}): {ex.Message}");
                }
            }
        }

        // Accept a request to cancel a real time data stream
        // Obviously we only actually cancel it if
        private void HandleRealTimeDataCancelRequest()
        {
            var buffer = requestSocket.ReceiveFrameBytes();
            // Receive the instrument
            using (var ms = new MemoryStream(buffer))
            {
                var instrument = MyUtils.ProtoBufDeserialize<Instrument>(ms);

                if (instrument != null)
                {
                    broker.CancelRtdStream(instrument.ID);
                }

                // Two part message:
                // 1: DataRequestMessageType.RTDCanceled
                // 2: the symbol
                requestSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.RTDCanceled));
                requestSocket.SendFrame(instrument?.Symbol);
            }
        }

        /// <summary>
        ///     Send an error reply to a real time data request.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="serializedRequest"></param>
        private void SendErrorReply(string message, byte[] serializedRequest)
        {
            requestSocket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Error));
            requestSocket.SendMoreFrame(message);
            requestSocket.SendFrame(serializedRequest);
        }

        #region Event handlers

        /// <summary>
        ///     When data arrives from an external data source to the broker, this event is fired.
        /// </summary>
        private void BrokerRealTimeDataArrived(object sender, RealTimeDataEventArgs e)
        {
            lock (publisherSocketLock)
            {
                if (publisherSocket != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, e);
                        publisherSocket.SendMoreFrame(BitConverter.GetBytes(e.InstrumentID));
                        // Start by sending the ticker before the data
                        publisherSocket.SendFrame(ms.ToArray()); // Then send the serialized bar
                    }
                }
            }
        }

        private void RequestSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            lock (requestConnectionString)
            {
                if (requestSocket != null)
                {
                    var requestType =
                        (DataRequestMessageType)BitConverter.ToInt16(requestSocket.ReceiveFrameBytes(), 0);

                    switch (requestType)
                    {
                        case DataRequestMessageType.CancelRTD:
                            logger.Info("Cancel RTD request received.");
                            HandleRealTimeDataCancelRequest();
                            break;

                        case DataRequestMessageType.RTDRequest:
                            logger.Info("RTD request received.");
                            HandleRealTimeDataRequest();
                            break;

                        case DataRequestMessageType.Ping:

                            requestSocket.SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Pong));
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        #endregion Event handlers
    }
}