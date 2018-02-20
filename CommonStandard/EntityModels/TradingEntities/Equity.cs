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
    public class Equity : IEntity
    {
        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public virtual Account Account { get; set; }
        [ProtoMember(3)]
        public int AccountID { get; set; }
        [ProtoMember(4)]
        [Required]
        public decimal Value { get; set; }
        [ProtoMember(5)]
        public DateTime UpdateTime { get; set; }

        

        #endregion
    }
}