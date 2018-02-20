using System;
using System.IO;
using Common.EntityModels;
using Common.Enums;
using Common.Interfaces;
using Common.Utils;
using NetMQ;
using NetMQ.Sockets;
using NLog;

namespace RequestsClientStandard
{
    public class RequestsClient : IRequestsClient
    {
        private NetMQPoller poller;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string pushConnectionString;

        private readonly object pushSocketLock = new object();
        private PushSocket pushSocket;
        private readonly string requestSocketConnectionString;

        public RequestsClient(int pushSocketPort, int requestSocketPort)
        {
            if (pushSocketPort > 0)
            {
                pushConnectionString = $"tcp://localhost:{pushSocketPort}";
            }
            else
            {
                throw new Exception("PushSocketPort must be greater than zero.");
            }

            if (requestSocketPort > 0)
            {
                requestSocketConnectionString = $"tcp://localhost:{requestSocketPort}";
            }
            else
            {
                throw new Exception("PushSocketPort must be greater than zero.");
            }
        }

        public void Connect()
        {
            lock (pushSocketLock)
            {
                pushSocket = new PushSocket(pushConnectionString);
                poller = new NetMQPoller() { pushSocket };
                poller.RunAsync();
            }
        }

        public void Disconnect()
        {
            pushSocket?.Disconnect(pushConnectionString);
            poller?.Stop();
            poller?.Dispose();
            pushSocket?.Dispose();
        }

        public void PushOrdersInfo(object objectToSend, PushMessageType messageType)
        {
            using (var ms = new MemoryStream())
            {
                var messageToSend = new NetMQMessage(2);
                messageToSend.Append(BitConverter.GetBytes((byte)messageType));
                messageToSend.Append(MyUtils.ProtoBufSerialize(objectToSend, ms));
                pushSocket.SendMultipartMessage(messageToSend);
            }
        }

        public Instrument RequestActiveInstrumentContract(int strategyID)
        {
            using (var requestSocket = new RequestSocket(requestSocketConnectionString))
            {
                var msg = new NetMQMessage(1);
                msg.Append(BitConverter.GetBytes((byte)RequestMessageType.InstrumentRequest));
                msg.Append(BitConverter.GetBytes(strategyID));
                logger.Info(() => "Sent request for new contract to server.");
                requestSocket.SendMultipartMessage(msg);
                //requestSocket.SendFrame(BitConverter.GetBytes(strategyID));
                logger.Info(() => "Waiting for new contract from server.");
                var response = requestSocket.ReceiveFrameBytes();
                using (var ms = new MemoryStream(response))
                {
                    logger.Info(() => "Contract received from server.");
                    return MyUtils.ProtoBufDeserialize<Instrument>(ms);
                }
            }
        }

        public Account RequestAccount(int strategyID)
        {
            using (var requestSocket = new RequestSocket(requestSocketConnectionString))
            {
                var msg = new NetMQMessage(1);
                msg.Append(BitConverter.GetBytes((byte)RequestMessageType.AccountRequest));
                msg.Append(BitConverter.GetBytes(strategyID));
                requestSocket.SendMultipartMessage(msg);
                // requestSocket.SendFrame(BitConverter.GetBytes(strategyID));
                logger.Info(() => "Waiting for account ID from server.");
                var response = requestSocket.ReceiveFrameBytes();
                using (var ms = new MemoryStream(response))
                {
                    logger.Info(() => "Account ID received from server.");
                    return MyUtils.ProtoBufDeserialize<Account>(ms);
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        public bool ClientRunningAndIsConnected => poller.IsRunning;
    }
}