using Common.Collections;
using Common.EntityModels;
using Common.EventArguments;
using Common.Requests;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface IHistoricalDataBroker : IDisposable
    {
        /// <summary>
        ///     Holds the historical data sources.
        /// </summary>
        ObservableDictionary<string, IHistoricalDataSource> DataSources { get; }

        void RequestHistoricalData(HistoricalDataRequest request);

        void AddData(DataAdditionRequest request);

        List<StoredDataInfo> GetAvailableDataInfo(Instrument instrument);

        event EventHandler<HistoricalDataEventArgs> HistoricalDataArrived;

        event EventHandler<ErrorArgs> Error;
    }
}