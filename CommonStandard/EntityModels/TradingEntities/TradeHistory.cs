using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class TradeHistory : IEntity
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string ExecutionID { get; set; }
        [ProtoMember(3)]
        public decimal Quantity { get; set; }
        [ProtoMember(4)]
        public TradeDirection Side { get; set; }
        [ProtoMember(5)]
        public virtual Instrument Instrument { get; set; }
        [ProtoMember(6)]
        public int InstrumentID { get; set; }
        [ProtoMember(7)]
        public decimal Price { get; set; }
        [ProtoMember(8)]
        public decimal Commission { get; set; }
        [ProtoMember(9)]
        public decimal RealizedPnL { get; set; }
        [ProtoMember(10)]
        public DateTime ExecutionTime { get; set; }
        [ProtoMember(11)]
        public virtual Account Account { get; set; }
        [ProtoMember(12)]
        public int AccountID { get; set; }

        
        #endregion
    }
}