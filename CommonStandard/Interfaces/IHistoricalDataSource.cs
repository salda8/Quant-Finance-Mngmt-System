using Common.EventArguments;
using Common.Requests;
using System;

namespace Common.Interfaces
{
    public interface IHistoricalDataSource : IDataSource
    {
        event EventHandler<HistoricalDataEventArgs> HistoricalDataArrived;

        void RequestHistoricalData(HistoricalDataRequest request);
    }
}