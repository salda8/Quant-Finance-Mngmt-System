using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class CommissionMessage : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string ExecutionId { get; set; }
        [ProtoMember(3)]
        public decimal Commission { get; set; }
        [ProtoMember(4)]
        public double RealizedPnL { get; set; }

    }

   }