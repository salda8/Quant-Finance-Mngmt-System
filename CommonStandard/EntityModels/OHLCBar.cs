using System;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using NodaTime;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class OHLCBar
    {
        [ProtoMember(1)]
        public decimal Open { get; set; }

        [ProtoMember(2)]
        public decimal High { get; set; }

        [ProtoMember(3)]
        public decimal Low { get; set; }

        [ProtoMember(4)]
        public decimal Close { get; set; }
        
        [ProtoMember(9)]
        [NotMapped]
        public long LongDate
        {
            get
            {
                return DateTimeClose.Ticks;
            }
            set
            {
                DateTimeClose = DateTime.FromBinary(value);
            }
        }

        [ProtoMember(91)]
        [NotMapped]
        public long? LongOpenDate
        {
            get
            {
                return DateTimeOpen?.Ticks;
            }
            set
            {
                DateTimeOpen = value.HasValue
                    ? DateTime.FromBinary(value.Value)
                    : (DateTime?)null;
            }
        }

        /// <summary>
        /// Date/Time of the bar open.
        /// </summary>
        public DateTime? DateTimeOpen { get; set; }

        /// <summary>
        /// Date/Time of the bar close.
        /// </summary>
        public DateTime DateTimeClose { get; set; }

        [ProtoMember(10)]
        public long? Volume { get; set; }
        
        [ProtoMember(12)]
        public int InstrumentID { get; set; }

        [ProtoMember(11)]
        public BarSize Frequency { get; set; }

        
    }
}
