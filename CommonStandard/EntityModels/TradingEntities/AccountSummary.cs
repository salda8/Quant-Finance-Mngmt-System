using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using DataAccess.EntityConfigs;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class AccountSummary : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public decimal NetLiquidation { get; set; }
        [ProtoMember(3)]
        public decimal CashBalance { get; set; }

        [ProtoMember(8)]
        public decimal UnrealizedPnL { get; set; }
        [ProtoMember(9)]
        public virtual Account Account { get; set; }
        [ProtoMember(10)]
        public int AccountID { get; set; }
        
    }
}
