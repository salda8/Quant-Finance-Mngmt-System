using Common.EventArguments;
using Common.Requests;
using System;

namespace Common.Interfaces
{
    public interface IRealTimeDataSource : IDataSource
    {
        /// <summary>
        /// Cancel a real time data stream.
        /// </summary>
        /// <param name="requestID">The ID of the real time data stream.</param>
        void CancelRealTimeData(int requestID);

        /// <summary>
        /// Fires when new real time data is received.
        /// </summary>
        event EventHandler<RealTimeDataEventArgs> DataReceived;

        int RequestRealTimeData(RealTimeDataRequest realTimeRequest);
    }
}