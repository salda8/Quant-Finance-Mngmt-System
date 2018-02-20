using System;
using System.IO;
using System.Threading.Tasks;
using Common;
using Common.EntityModels;
using Common.Enums;
using Common.Interfaces;
using Common.Utils;
using DataAccess.EntityConfigs;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using Server.Repositories;
using Server.Utils;

namespace Server.Servers
{
    public class MessagesServer : INetMQServer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string publisherConnectionString;
        private readonly object publisherSocketLock = new object();
        private readonly string pullSocketConnectionString;
        private readonly object pullSocketLock = new object();
        private NetMQPoller poller;
        private NetMQSocket publisherSocket;
        private NetMQSocket pullSocket;
        private readonly MessagesRepository repository;

        public MessagesServer(int pushPort, string host = "localhost", string transportProtocol = "tcp") : this(new MqServer(pushPort, host, (TransportProtocol)Enum.Parse(typeof(TransportProtocol), transportProtocol)))
        {
            
        }

        public MessagesServer(MqServer server)
        {
            //if (publisher.GetAddress() == pull.GetAddress())
            //{
            //    throw new ArgumentException("Publish and request ports must be different");
            //}

            //publisherConnectionString = publisher.GetAddress();
            if (!ServerUtils.IsPortOpen(server.HostIp, server.Port, TimeSpan.FromSeconds(2)))
            {
                throw new PortIsInUseException($"{server.GetAddress()} is in use.");
            }
            pullSocketConnectionString = server.GetAddress();

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
            {
                return;
            }

            //lock (publisherSocketLock)
            //{
            //    publisherSocket = new PushSocket(publisherConnectionString);
            //}

            lock (pullSocketLock)
            {
                pullSocket = new PullSocket(pullSocketConnectionString);
                pullSocket.ReceiveReady += PullSocketReceiveReady;
                logger.Log(LogLevel.Info, $"Pull socket established on {pullSocketConnectionString}");
                poller = new NetMQPoller { pullSocket };
                poller.RunAsync();
            }


            
        }

        private void PullSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            lock (pullSocketLock)
            {
                if (pullSocket != null)
                {
                    var message = pullSocket.ReceiveMultipartMessage(2);
                    if (message.FrameCount < 2)
                    {
                        logger.Error("Messages Server: Received corrupted message.");
                        return;
                    }

                    var receivedObject = message[1].Buffer;

                    switch ((PushMessageType) BitConverter.ToInt16(message[0].Buffer, 0))
                    {
                        case PushMessageType.OpenOrderPush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    repository.AddNewOpenOrderMessage(
                                        MyUtils.ProtoBufDeserialize<OpenOrder>(ms));
                                }
                            });
                            break;

                        case PushMessageType.CommissionPush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    var commissionMessage =
                                        MyUtils.ProtoBufDeserialize<CommissionMessage>(ms);
                                    repository.AddCommissionMessage(commissionMessage);
                                }
                            });
                            break;

                        case PushMessageType.ExecutionPush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    repository.AddExecutionMessage(
                                        MyUtils.ProtoBufDeserialize<ExecutionMessage>(ms));
                                }
                            });
                            break;

                        case PushMessageType.OrderStatusPush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    repository.AddNewOrderStatusMessage(
                                        MyUtils.ProtoBufDeserialize<OrderStatusMessage>(ms));
                                }
                            });
                            break;
                        case PushMessageType.LiveTradePush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    repository.AddOrUpdateLiveTrade(
                                        MyUtils.ProtoBufDeserialize<LiveTrade>(ms));
                                }
                            });
                            break;

                        case PushMessageType.AccountUpdatePush:
                            Task.Factory.StartNew(() =>
                            {
                                using (var ms = new MemoryStream(receivedObject))
                                {
                                    repository.UpdateAccountSummary(
                                        MyUtils.ProtoBufDeserialize<AccountSummaryUpdate>(ms));
                                }
                            });
                            break;
                        case PushMessageType.EquityUpdatePush:
                            Task.Factory.StartNew(() =>
                           {
                               using (var ms = new MemoryStream(receivedObject))
                               {
                                   repository.UpdateEquity(
                                       MyUtils.ProtoBufDeserialize<Equity>(ms));
                               }
                           });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
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

            lock (pullSocketLock)
            {
                if (pullSocket != null)
                {
                    try
                    {
                        pullSocket.Disconnect(pullSocketConnectionString);
                    }
                    finally
                    {
                        // pullSocket.ReceiveReady -= RequestSocketReceiveReady;
                        pullSocket.Close();
                        pullSocket = null;
                    }
                }
            }

            poller = null;
        }
    }
}