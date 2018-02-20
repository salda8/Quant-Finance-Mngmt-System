using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class OpenOrder : IEntity
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public int PermanentId { get; set; }
        [ProtoMember(3)]
        public virtual Instrument Instrument { get; set; }
        [ProtoMember(4)]
        public int InstrumentID { get; set; }
        [ProtoMember(5)]
        public string Status { get; set; }
        [ProtoMember(6)]
        public decimal LimitPrice { get; set; }
        [ProtoMember(7)]
        public decimal Quantity { get; set; }
        [ProtoMember(8)]
        public virtual Account Account { get; set; }
        [ProtoMember(9)]
        public int AccountID { get; set; }
        [ProtoMember(10)]
        public DateTime UpdateTime { get; set; }
        [ProtoMember(11)]
        public string Type { get; set; }
        [ProtoMember(12)]
        [NotMapped]
        public int OrderId { get; set; }

        #endregion
    }
}