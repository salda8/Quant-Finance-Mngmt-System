using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class LiveTrade : IEntity
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public virtual Instrument Instrument { get; set; }
        [ProtoMember(3)]
        public int InstrumentID { get; set; }
        [ProtoMember(4)]
        public decimal Quantity { get; set; }
        [ProtoMember(5)]
        public TradeDirection TradeDirection { get; set; }
        [ProtoMember(6)]
        public decimal MarketPrice { get; set; }

        [ProtoMember(8)]
        public decimal AveragePrice { get; set; }
        [ProtoMember(9)]
        public decimal UnrealizedPnL { get; set; }
        [ProtoMember(10)]
        public decimal RealizedPnl { get; set; }
        [ProtoMember(11)]
        public virtual Account Account { get; set; }
        [ProtoMember(12)]
        public int AccountID { get; set; }
        [ProtoMember(13)]
        public virtual DateTime UpdateTime { get; set; }
        [ProtoMember(14)]
        public int Port { get; set; }

        #endregion
    }
}