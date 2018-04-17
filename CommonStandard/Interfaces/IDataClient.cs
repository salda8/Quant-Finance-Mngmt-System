using System;
using System.Collections.ObjectModel;
using Common.EntityModels;
using Common.EventArguments;
using Common.Requests;

namespace Common.Interfaces
{
    public interface IDataClient : INetMQClient
    {
        
      ObservableCollection<RealTimeDataRequest> RealTimeDataStreams { get; }

        event EventHandler<ErrorArgs> Error;
       
       
        event EventHandler<RealTimeDataEventArgs> RealTimeDataReceived;

        void CancelRealTimeData(Instrument instrument);
        
        
       
        int RequestRealTimeData(RealTimeDataRequest request);
    }
}