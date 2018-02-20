// This class handles networking for historical data.
// Clients send their requests through ZeroMQ. Here they are parsed
// and then forwarded to the HistoricalDataBroker.
// Data sent from the HistoricalDataBroker is sent out to the clients.
// Three types of possible requests:
// 1. For historical data
// 2. To check what data is available in the local database
// 3. To add data to the local database

using Common;
using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Interfaces;
using Common.Requests;
using Common.Utils;
using LZ4;
using NetMQ;
using NetMQ.Sockets;
using NLog;

using System;
using System.Collections.Generic;
using System.IO;
using DataAccess.EntityConfigs;
using ProtoBuf;
using Server.Servers;
using Server.Utils;

namespace Server.Servers
{
    public class HistoricalDataServer : INetMQServer
    {
        private readonly IHistoricalDataBroker broker;
        private readonly string connectionString;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly object socketLock = new object();
        private NetMQPoller poller;

        private NetMQSocket socket;

        public HistoricalDataServer(int port, IHistoricalDataBroker broker, string host = "localhost", string transportProtocol = "tcp") :  this(new MqServer(port, host, (TransportProtocol) Enum.Parse(typeof(TransportProtocol), transportProtocol)), broker)
        {

            
        }

        public HistoricalDataServer(MqServer server, IHistoricalDataBroker broker)
        {
            if (!ServerUtils.IsPortOpen(server.HostIp, server.Port, TimeSpan.FromSeconds(2)))
            {
                throw new PortIsInUseException($"{server.GetAddress()} is in use.");
            }
            connectionString = server.GetAddress();
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker), $"{nameof(broker)} cannot be null");
            this.broker.HistoricalDataArrived += BrokerHistoricalDataArrived;
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
        ///     Start the server.
        /// </summary>
        public void StartServer()
        {
            if (ServerRunning)
                return;

            lock (socketLock)
            {
                socket = new RouterSocket(connectionString);
                socket.ReceiveReady += SocketReceiveReady;
                poller = new NetMQPoller { socket };
                poller.RunAsync();
            }

           
        }

        /// <summary>
        ///     Stop the server.
        /// </summary>
        public void StopServer()
        {
            if (!ServerRunning)
                return;

            poller?.Stop();
            poller?.Dispose();

            lock (socketLock)
            {
                if (socket != null)
                {
                    try
                    {
                        socket.Disconnect(connectionString);
                    }
                    finally
                    {
                        socket.ReceiveReady -= SocketReceiveReady;
                        socket.Close();
                        socket = null;
                    }
                }
            }

            poller = null;
        }

        /// <summary>
        ///     Given a historical data request and the data that fill it,
        ///     send the reply to the client who made the request.
        /// </summary>
        private void SendFilledHistoricalRequest(HistoricalDataRequest request, List<OHLCBar> data)
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        // This is a 5 part message
                        // 1st message part: the identity string of the client that we're routing the data to
                        var clientIdentity = request.RequesterIdentity;

                        socket.SendMoreFrame(clientIdentity ?? string.Empty);
                        // 2nd message part: the type of reply we're sending
                        socket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.HistReply));
                        // 3rd message part: the HistoricalDataRequest object that was used to make the request
                        socket.SendMoreFrame(MyUtils.ProtoBufSerialize(request, ms));
                        // 4th message part: the size of the uncompressed, serialized data. Necessary for decompression on the client end.
                        var uncompressed = MyUtils.ProtoBufSerialize(data, ms);

                        socket.SendMoreFrame(BitConverter.GetBytes(uncompressed.Length));
                        // 5th message part: the compressed serialized data.
                        var compressed = LZ4Codec.EncodeHC(uncompressed, 0, uncompressed.Length); // compress

                        socket.SendFrame(compressed);
                    }
                }
            }
        }

        /// <summary>
        ///     Handles requests for information on data that is available in local storage
        /// </summary>
        private void AcceptAvailableDataRequest(string requesterIdentity)
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    // Get the instrument
                    byte[] buffer = socket.ReceiveFrameBytes();
                    Instrument instrument;
                    using (var ms = new MemoryStream(buffer))
                    {
                        instrument = MyUtils.ProtoBufDeserialize<Instrument>(ms);
                    }

                    logger.Info($"Received local data storage info request for {instrument.Symbol}.");
                    // And send the reply
                    List<StoredDataInfo> storageInfo = broker.GetAvailableDataInfo(instrument);

                    socket.SendMoreFrame(requesterIdentity);
                    socket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.AvailableDataReply));
                    using (var ms = new MemoryStream(buffer))
                    {
                        socket.SendMoreFrame(MyUtils.ProtoBufSerialize(instrument, ms));
                        socket.SendMoreFrame(BitConverter.GetBytes(storageInfo.Count));

                        for (var i = 0; i < storageInfo.Count; i++)
                        {
                            var sdi = storageInfo[i];
                            if (i < storageInfo.Count - 1)
                                socket.SendMoreFrame(MyUtils.ProtoBufSerialize(sdi, ms));
                            else
                                socket.SendFrame(MyUtils.ProtoBufSerialize(sdi, ms));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Handles incoming data "push" requests: the client sends data for us to add to local storage.
        /// </summary>
        private void AcceptDataAdditionRequest(string requesterIdentity)
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    var buffer = socket.ReceiveFrameBytes();
                    DataAdditionRequest request;
                    using (var ms = new MemoryStream(buffer))
                    {
                        // Final message part: receive the DataAdditionRequest object

                        request = MyUtils.ProtoBufDeserialize<DataAdditionRequest>(ms);
                    }

                    logger.Info($"Received data push request for {request.Instrument.Symbol}.");
                    // Start building the reply
                    socket.SendMoreFrame(requesterIdentity);
                    socket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.HistPushReply));

                    try
                    {
                        broker.AddData(request);

                        socket.SendFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Success));
                    }
                    catch (Exception ex)
                    {
                        socket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Error));
                        socket.SendFrame(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        ///     Processes incoming historical data requests.
        /// </summary>
        private void AcceptHistoricalDataRequest(string requesterIdentity)
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    // Third: a serialized HistoricalDataRequest object which contains the details of the request
                    var buffer = socket.ReceiveFrameBytes();

                    using (var ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, buffer.Length);
                        ms.Position = 0;

                        var request = Serializer.Deserialize<HistoricalDataRequest>(ms);

                        logger.Info(
                            "Historical Data Request from client {0}: {7} {1} @ {2} from {3} to {4} Location: {5} {6:;;SaveToLocal}",
                            requesterIdentity,
                            request.Instrument.Symbol,
                            Enum.GetName(typeof(BarSize), request.Frequency),
                            request.StartingDate,
                            request.EndingDate,
                            request.DataLocation,
                            request.SaveToLocalStorage ? 0 : 1,
                            request.Instrument.Datasource.Name);

                        request.RequesterIdentity = requesterIdentity;

                        try
                        {
                            broker.RequestHistoricalData(request);
                        }
                        catch (Exception ex)
                        {
                            // There's some sort of problem with fulfilling the request. Inform the client.
                            SendErrorReply(requesterIdentity, request.RequestID, ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     If a historical data request can't be filled,
        ///     this method sends a reply with the relevant error.
        /// </summary>
        private void SendErrorReply(string requesterIdentity, int requestId, string message)
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    // This is a 4 part message
                    // 1st message part: the identity string of the client that we're routing the data to
                    socket.SendMoreFrame(requesterIdentity);
                    // 2nd message part: the type of reply we're sending
                    socket.SendMoreFrame(BitConverter.GetBytes((byte)DataRequestMessageType.Error));
                    // 3rd message part: the request ID
                    socket.SendMoreFrame(BitConverter.GetBytes(requestId));
                    // 4th message part: the error
                    socket.SendFrame(message);
                }
            }
        }

        #region Event handlers

        private void BrokerHistoricalDataArrived(object sender, HistoricalDataEventArgs e)
        {
            SendFilledHistoricalRequest(e.Request, e.Data);
        }

        /// <summary>
        ///     This is called when a new historical data request or data push request is made.
        /// </summary>
        private void SocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            string requesterIdentity;
            DataRequestMessageType type;

            lock (socketLock)
            {
                // Here we process the first two message parts: first, the identity string of the client
                requesterIdentity = socket.ReceiveFrameString() ?? string.Empty;
                // Second: the string specifying the type of request
                type = (DataRequestMessageType)BitConverter.ToInt16(socket.ReceiveFrameBytes(), 0);
            }

            switch (type)
            {
                case DataRequestMessageType.HistPush:
                    AcceptDataAdditionRequest(requesterIdentity);
                    break;

                case DataRequestMessageType.HistRequest:
                    AcceptHistoricalDataRequest(requesterIdentity);
                    break;

                case DataRequestMessageType.AvailableDataRequest:
                    AcceptAvailableDataRequest(requesterIdentity);
                    break;

                default:
                    logger.Info($"Unrecognized request to historical data broker: {type}");
                    break;
            }
        }

        #endregion Event handlers
    }
}