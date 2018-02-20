using System;
using Common.EntityModels;
using Common.Enums;
using ProtoBuf;

namespace Common.Requests
{
    [ProtoContract]
  
    public class RealTimeDataRequest
    {

        [ProtoMember(1)]
        public BarSize Frequency { get; set; }

        /// <summary>
        /// Regular trading hours data only.
        /// </summary>
        [ProtoMember(2)]
        public bool RTHOnly { get; set; }

        [ProtoMember(3)]
        public Instrument Instrument { get; set; }

        /// <summary>
        /// Save incoming data to local storage.
        /// </summary>
        [ProtoMember(4)]
        public bool SaveToLocalStorage { get; set; }

        /// <summary>
        /// This value is used on the client side to uniquely identify real time data requests.
        /// </summary>
        [ProtoMember(5)]
        public int RequestID { get; set; }

        /// <summary>
        /// The real time data broker gives the request an ID, which is then used to identify it when the data is returned.
        /// </summary>
        public int AssignedID { get; set; }
        

        public RealTimeDataRequest()
        {
        }

        public RealTimeDataRequest(Instrument instrument, BarSize frequency, bool rthOnly = true, bool savetoLocalStorage = false)
        {
            Instrument = instrument;
            Frequency = frequency;
            RTHOnly = rthOnly;
            SaveToLocalStorage = savetoLocalStorage;
        }

    }

    
}
