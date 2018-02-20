using Common.EntityModels;
using Common.Enums;
using Common.Interfaces;
using Common.Utils;
using DataAccess;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using System;

using System.IO;
using System.Linq;
using DataAccess.EntityConfigs;
using Microsoft.EntityFrameworkCore;
using Server.Utils;

namespace Server.Servers
{
    public class RequestResponseServer : INetMQServer
    {
        private readonly string responseSocketConnectionString;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ResponseSocket responseSocket;
        private readonly object responseSocketLock = new object();
        private NetMQPoller poller;

        public RequestResponseServer(int responseSocketPort, string host = "localhost", string responseTransportProtocol = "tcp") : this(new MqServer(responseSocketPort, host, (TransportProtocol)Enum.Parse(typeof(TransportProtocol), responseTransportProtocol)))
        {
            
        }

        public RequestResponseServer(MqServer server)
        {
            if (!ServerUtils.IsPortOpen(server.HostIp, server.Port, TimeSpan.FromSeconds(2)))
            {
                throw new PortIsInUseException($"{server.GetAddress()} is in use.");
            }
            responseSocketConnectionString = server.GetAddress();
        }

        public void Dispose()
        {
            StopServer();
        }

        public bool ServerRunning => poller?.IsRunning ?? false;

        public void StartServer()
        {
            if (ServerRunning)
            {
                return;
            }

            lock (responseSocketLock)
            {
                responseSocket = new ResponseSocket(responseSocketConnectionString);
                responseSocket.ReceiveReady += ResponseSocketReceiveReady;
                logger.Log(LogLevel.Info, $"Response socket established on {responseSocketConnectionString}");

                poller = new NetMQPoller { responseSocket };
                poller.RunAsync();
            }

        }

        private void ResponseSocketReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var message = e.Socket.ReceiveMultipartMessage(2);
            if (message.FrameCount < 1)
            {
                logger.Error("Messages Server: Received corrupted message.");
                return;
            }

            RequestMessageType type = (RequestMessageType)BitConverter.ToInt16(message[0].Buffer, 0);
            int strategyId = BitConverter.ToInt32(message[1].Buffer, 0);

            switch (type)
            {
                case RequestMessageType.InstrumentRequest:
                    NewInstrumentContractRequested(strategyId);
                    break;

                case RequestMessageType.AccountRequest:
                    AccountRequest(strategyId);
                    break;

                default:
                    break;
            }
        }

        private void AccountRequest(int strategyId)
        {
            using (var context = new MyDBContext())
            {
                Account account = context.Account.SingleOrDefault(x => x.StrategyID == strategyId);

                using (var ms = new MemoryStream())
                {
                    responseSocket.SendFrame(MyUtils.ProtoBufSerialize(account, ms));
                }
            }
        }

        private void NewInstrumentContractRequested(int strategyId)
        {
            using (var context = new MyDBContext())
            {
                var oldInstrument =
                    context.Strategy.Where(x => x.ID == strategyId)
                        .Select(x => x.Instrument)
                        .Include("ExpirationRule")
                        .Include("Datasource")
                        .Include("Exchange")
                        .Include("Sessions")
                        .Single();
                Instrument newInstrument = null;
                if (IsInstrumentCloseToBeExpiredOrExpired(oldInstrument))
                {
                    newInstrument =
                        context.Instruments.Where(
                                x =>
                                    x.UnderlyingSymbol == oldInstrument.UnderlyingSymbol &&
                                    x.Expiration > oldInstrument.Expiration)
                            .Include("ExpirationRule")
                            .Include("Datasource")
                            .Include("Exchange")
                            .Include("Sessions")
                            .OrderByDescending(x => x.Expiration)
                            .FirstOrDefault();
                }
                else
                {
                    newInstrument = oldInstrument;
                }
                
                using (var ms = new MemoryStream())
                {
                    responseSocket.SendFrame(MyUtils.ProtoBufSerialize(newInstrument, ms));
                }
            }
        }

        private static bool IsInstrumentCloseToBeExpiredOrExpired(Instrument oldInstrument)
        {
            return ((DateTime.Now.DayOfYear - (oldInstrument.Expiration.DayOfYear - oldInstrument.ExpirationRule.DaysBefore)) >= oldInstrument.ExpirationRule.DaysBefore);
        }

        public void StopServer()
        {
            if (!ServerRunning)
                return;

            poller?.Stop();
            poller?.Dispose();

            lock (responseSocketLock)
            {
                if (responseSocket != null)
                {
                    try
                    {
                        responseSocket.Disconnect(responseSocketConnectionString);
                    }
                    finally
                    {
                        responseSocket.Close();
                        responseSocket = null;
                    }
                }
            }
        }
    }
}