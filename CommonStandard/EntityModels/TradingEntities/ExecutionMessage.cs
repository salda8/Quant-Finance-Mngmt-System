using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using DataAccess.EntityConfigs;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class ExecutionMessage : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public int RequestId { get; set; }
        [ProtoMember(3)]
        public string ExecutionId { get; set; }
        [ProtoMember(4)]
        public virtual Account Account { get; set; }
        [ProtoMember(5)]
        public int AccountID { get; set; }
        [ProtoMember(6)]
        public int PermanentId { get; set; }
        [ProtoMember(7)]
        public int OrderId { get; set; }
        [ProtoMember(8)]
        public virtual Instrument Instrument { get; set; }
        [ProtoMember(9)]
        public int InstrumentID { get; set; }
        [ProtoMember(10)]
        public int Quantity { get; set; }
        [ProtoMember(11)]
        public string Side { get; set; }
        [ProtoMember(12)]
        public decimal Price { get; set; }
        [ProtoMember(13)]
        public DateTime Time { get; set; }

       

    }

   
}