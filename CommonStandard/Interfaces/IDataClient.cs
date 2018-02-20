using System;
using System.Collections.ObjectModel;
using Common.EntityModels;
using Common.EventArguments;
using Common.Requests;

namespace Common.Interfaces
{
    public interface IDataClient : INetMQClient
    {
        
        ObservableCollection<HistoricalDataRequest> PendingHistoricalRequests { get; }
        ObservableCollection<RealTimeDataRequest> RealTimeDataStreams { get; }

        event EventHandler<ErrorArgs> Error;
        event EventHandler<HistoricalDataEventArgs> HistoricalDataReceived;
        event EventHandler<LocallyAvailableDataInfoReceivedEventArgs> LocallyAvailableDataInfoReceived;
        event EventHandler<RealTimeDataEventArgs> RealTimeDataReceived;

        void CancelRealTimeData(Instrument instrument);
        
        void GetLocallyAvailableDataInfo(Instrument instrument);
        void PushData(DataAdditionRequest request);
        int RequestHistoricalData(HistoricalDataRequest request);
        int RequestRealTimeData(RealTimeDataRequest request);
    }
}