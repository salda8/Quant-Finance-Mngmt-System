using Common.Enums;
using Common.Interfaces;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.EntityConfigs;

namespace Common.EntityModels
{
    [ProtoContract]
    public class InstrumentSession : ISession, IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }

        
        public TimeSpan OpeningTime { get; set; }

       
        public TimeSpan ClosingTime { get; set; }

        [ProtoMember(2)]
        public int InstrumentID { get; set; }


        public virtual Instrument Instrument { get; set; } 


        [ProtoMember(3)]
        [NotMapped]
        public double OpeningAsSeconds
        {
            get
            {
                return OpeningTime.TotalSeconds;
            }
            set
            {
                OpeningTime = TimeSpan.FromSeconds(value);
            }
        }

        [ProtoMember(4)]
        [NotMapped]
        public double ClosingAsSeconds
        {
            get
            {
                return ClosingTime.TotalSeconds;
            }
            set
            {
                ClosingTime = TimeSpan.FromSeconds(value);
            }
        }

        [ProtoMember(5)]
        public bool IsSessionEnd { get; set; }

        [ProtoMember(6)]
        public DayOfTheWeek OpeningDay { get; set; }

        [ProtoMember(7)]
        public DayOfTheWeek ClosingDay { get; set; }

       
    }
}