using Common.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Common.EntityModels
{
    [ProtoContract]
    public class Exchange : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [ProtoMember(3)]
        [MaxLength(255)]
        public string Timezone { get; set; }


        [ProtoMember(4)]
        [MaxLength(255)]
        public string LongName { get; set; }
        

        public override string ToString()
        {
            return $"{ID} {Name} ({LongName}) TZ: {Timezone}";
        }
    }
}