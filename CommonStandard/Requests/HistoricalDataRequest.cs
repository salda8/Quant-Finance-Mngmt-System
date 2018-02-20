using System;
using Common.EntityModels;
using Common.Enums;
using ProtoBuf;

namespace Common.Requests
{
    [ProtoContract]
    public class HistoricalDataRequest
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
        /// <summary>
        /// Inclusive starting date for the period requested.
        /// </summary>
        public DateTime StartingDate
        {
            get
            {
                return new DateTime(longStartingDate);
            }
            set
            {
                longStartingDate = value.Ticks;
            }
        }

        /// <summary>
        /// Inclusive ending date for the period requested.
        /// </summary>
        public DateTime EndingDate
        {
            get
            {
                return new DateTime(longEndingDate);
            }
            set
            {
                longEndingDate = value.Ticks;
            }
        }
        
        [ProtoMember(6)]
        private long longStartingDate;

        [ProtoMember(7)]
        private long longEndingDate;

        /// <summary>
        /// Determines where the data will be downloaded from:
        /// Local only, external only (force fresh download), 
        /// or both (data not available locally will be downloaded)
        /// </summary>
        [ProtoMember(8)]
        public DataLocation DataLocation { get; set; }
        
        /// <summary>
        /// This property references the "parent" request's AssignedID
        /// </summary>
        [ProtoMember(9)]
        public int? IsSubrequestFor { get; set; }

        /// <summary>
        /// The server assigns the requester's zeromq identity string to this property 
        /// so the data can be sent back to the correct client when it arrives.
        /// </summary>
        public string RequesterIdentity { get; set; }

        public HistoricalDataRequest()
        {
        }

        public HistoricalDataRequest(Instrument instrument, BarSize frequency, DateTime startingDate, DateTime endingDate, DataLocation dataLocation = DataLocation.Both, bool saveToLocalStorage = true, bool rthOnly = true, int requestID = 0)
        {
            Frequency = frequency;
            Instrument = instrument;
            StartingDate = startingDate;
            EndingDate = endingDate;
            DataLocation = dataLocation;
            SaveToLocalStorage = saveToLocalStorage;
            RTHOnly = rthOnly;
            RequestID = requestID;
        }

        
    }
}
