using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{   
    [ProtoContract]
    public class OrderStatusMessage : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public int OrderId { get; set; }
        [ProtoMember(3)]
        public string Status { get; set; }
        [ProtoMember(4)]
        public int Filled { get; set; }
        [ProtoMember(5)]
        public int Remaining { get; set; }
        [ProtoMember(6)]
        public decimal AverageFillPrice { get; set; }
        [ProtoMember(7)]
        public int PermanentId { get; set; }
        [ProtoMember(8)]
        public int ParentId { get; set; }
        [ProtoMember(9)]
        public decimal LastFillPrice { get; set; }
        [ProtoMember(10)]
        public int ClientId { get; set; }
        [ProtoMember(11)]
        public string WhyHeld { get; set; }
    }
}