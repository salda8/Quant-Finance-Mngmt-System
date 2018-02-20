// The idea behind the way expiration rules work is to first find
// the "reference day", and then use an offset from that day to find
// the actual expiration date.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Enums;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    /// <summary>
    /// This class holds a set of rules that collectively can be used to deduce the expiration date of a futures or options contract.
    /// </summary>
    [ProtoContract]
    public class ExpirationRule : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        
        /// <summary>
        /// The future expires this many days before the expiration day.
        /// </summary>
        [ProtoMember(2)]
        public int DaysBefore { get; set; }
        
       
        [ProtoMember(3)]
        public string Name { get; set; }

        
    }
}