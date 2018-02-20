using System;
using System.Collections.Generic;
using System.Text;
using Common.Interfaces;
using DataAccess.EntityConfigs;
using Server.Servers;

namespace ServerStandard.Servers
{
    public class ServerFactory
    {
        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns><c>true</c> if Server is successfully started, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">server</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public bool StartServer(MqServer server)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            INetMQServer netMqServer;
            switch (server.ServerType)
            {
                case ServerType.EquityUpdate:
                    netMqServer = new EquityUpdateServer(server);
                    netMqServer.StartServer();
                    break;
                case ServerType.HistoricalData:
                    netMqServer = new HistoricalDataServer(server, null);
                    netMqServer.StartServer();
                    break;
                case ServerType.InstrumentRequest:
                    netMqServer = new RequestResponseServer(server);
                    netMqServer.StartServer();
                    break;
                case ServerType.Message:
                    netMqServer = new MessagesServer(server);
                    netMqServer.StartServer();
                    break;
                case ServerType.RealTimeData:
                    netMqServer = new EquityUpdateServer(server);
                    netMqServer.StartServer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return netMqServer.ServerRunning;
        }
    }
}
