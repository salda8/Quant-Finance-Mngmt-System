using System;

namespace Common.Interfaces
{
    public interface INetMQServer : IDisposable
    {
        bool ServerRunning { get; }
        
        void StartServer();
        void StopServer();
    }

    public interface INetMQClient : IDisposable
    {
        bool ClientRunningAndIsConnected { get; }

        void Connect();
        void Disconnect();
    }
}