using Common.Collections;
using Common.EventArguments;
using Common.Requests;
using System;

namespace Common.Interfaces
{
    public interface IRealTimeDataBroker
    {
        /// <summary>
        ///     Holds the real time data sources.
        /// </summary>
        ObservableDictionary<string, IRealTimeDataSource> DataSources { get; }

        ConcurrentNotifierBlockingList<RealTimeStreamInfo> ActiveStreams { get; }

        event EventHandler<RealTimeDataEventArgs> RealTimeDataArrived;

        bool RequestRealTimeData(RealTimeDataRequest request);

        bool CancelRtdStream(int instrumentID);
    }
}