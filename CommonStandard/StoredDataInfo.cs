using Common.Enums;
using ProtoBuf;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common
{
    [ProtoContract]
    public class StoredDataInfo
    {
        [ProtoMember(1)]
        public int InstrumentID { get; set; }

        [ProtoMember(2)]
        public BarSize Frequency { get; set; }

        public DateTime EarliestDate { get; set; }

        public DateTime LatestDate { get; set; }

        [ProtoMember(3)]
        [NotMapped]
        public long EarliestDateAsLong
        {
            get
            {
                return EarliestDate.Ticks;
            }
            set
            {
                EarliestDate = DateTime.FromBinary(value);
            }
        }

        [ProtoMember(4)]
        [NotMapped]
        public long LatestDateAsLong
        {
            get
            {
                return LatestDate.Ticks;
            }
            set
            {
                LatestDate = DateTime.FromBinary(value);
            }
        }
    }
}