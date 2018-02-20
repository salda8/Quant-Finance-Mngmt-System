using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using DataAccess.EntityConfigs;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class Account : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string AccountNumber { get; set; }
        [ProtoMember(3)]
        public string BrokerName { get; set; }

        [ProtoMember(6)]
        public decimal InitialBalance { get; set; }
        
        public virtual Strategy Strategy { get; set; }
        [ProtoMember(8)]
        public int StrategyID { get; set; }

        


    }
}
