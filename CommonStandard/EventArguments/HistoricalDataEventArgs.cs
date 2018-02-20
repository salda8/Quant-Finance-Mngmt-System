using Common.EntityModels;
using Common.Requests;
using System;
using System.Collections.Generic;

namespace Common.EventArguments
{
    public class HistoricalDataEventArgs : EventArgs
    {
        /// <summary>
        /// Historical data event args.
        /// </summary>
        public HistoricalDataEventArgs(HistoricalDataRequest request, List<OHLCBar> data)
        {
            Request = request;
            Data = data;
        }

        private HistoricalDataEventArgs()
        {
        }

        /// <summary>
        /// The request that is being filled.
        /// </summary>
        public HistoricalDataRequest Request;

        /// <summary>
        /// The data.
        /// </summary>
        public List<OHLCBar> Data;
    }
}