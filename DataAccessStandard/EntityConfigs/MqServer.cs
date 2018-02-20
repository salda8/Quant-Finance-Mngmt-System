using Common.Interfaces;

namespace DataAccess.EntityConfigs
{
    public class MqServer : IEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public MqServer()
        {
            
        }

        public MqServer(int port, string hostIp, TransportProtocol transportProtocol)
        {
            Port = port;
            HostIp = hostIp;
            TransportProtocol = transportProtocol;
        }

        public ServerType ServerType{
            get;
            set;
        }

        public int Port { get; set; }
        public string HostIp { get; set; }
        public TransportProtocol TransportProtocol { get; set; }

        public string GetAddress()
        {
            return TransportProtocol == TransportProtocol.tcp ? $"tcp://{HostIp}:{Port}"
                : $"{TransportProtocol.ToString()}://{HostIp}";
            
        }
    }
}