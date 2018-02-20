using Common.Interfaces;
using Common.Utils;
using DataAccess;
using NetMQ;
using NetMQ.Sockets;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using DataAccess.EntityConfigs;
using Microsoft.EntityFrameworkCore;
using Server.Utils;

namespace Server.Servers
{
    public class EquityUpdateServer : INetMQServer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string routerConnectionString;
        private bool running;
        private readonly RouterSocket routerSocket;

        public EquityUpdateServer(int routerSocketPort, string host = "localhost", string transportProtocol = "tcp") : this(new MqServer(routerSocketPort, host, (TransportProtocol)Enum.Parse(typeof(TransportProtocol), transportProtocol)))
        {
            
        }

        public EquityUpdateServer(MqServer server)
        {
            if (!ServerUtils.IsPortOpen(server.HostIp, server.Port, TimeSpan.FromSeconds(2)))
            {
                throw new PortIsInUseException($"{server.GetAddress()} is in use.");
            }



            routerConnectionString = server.GetAddress();
            routerSocket = new RouterSocket();
        }
    

    /// <summary>
        ///     Whether the server is running or not.
        /// </summary>
        public bool ServerRunning => running;

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
            using (routerSocket)
            {
                Thread.Sleep(10000);
                logger.Log(LogLevel.Info, $"Trying to bind to {routerConnectionString}");
                routerSocket.Bind(routerConnectionString);
                running = true;
                logger.Log(LogLevel.Info, $"Router socket successfully binded on {routerConnectionString}");
                Console.WriteLine($"Server is bind to {routerConnectionString}");
                while (running)
                {
                    logger.Log(LogLevel.Info, $"Router socket is waiting on message on {routerConnectionString}");
                    var message = routerSocket.ReceiveMultipartMessage();
                    var accountID = message[2].ConvertToInt32();
                    logger.Log(LogLevel.Info, $"Received request for equity on account ID:{accountID}");

                    var response = new NetMQMessage();

                    using (var ms = new MemoryStream())
                    {
                        var input = new MyDBContext().Equity.FromSql($"SELECT value FROM dbo.Equity WHERE AccountID={accountID}").Last();
                        var equity = MyUtils.ProtoBufSerialize(input, ms);
                        response.Append(message[0]);
                        response.AppendEmptyFrame();
                        response.Append(equity);
                        routerSocket.SendMultipartMessage(response);
                    }
                }
            }
        }

        /// <summary>
        ///     Stops the server.
        /// </summary>
        public void StopServer()
        {
            running = false;
        }
    }
}