using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Interfaces;
using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class Strategy : IEntity
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(1)]
        public int ID { get; set; }
        [ProtoMember(2)]
        public string StrategyName { get; set; }

        [ProtoMember(5)]
        public virtual Instrument Instrument { get; set; }
        [ProtoMember(6)]
        public int InstrumentID { get; set; }
        [ProtoMember(7)]
        public decimal BacktestPeriodInYears { get; set; }
        [ProtoMember(8)]
        public decimal BacktestDrawDown { get; set; }
        [ProtoMember(9)]
        public decimal BacktestProfit { get; set; }

        #endregion
    }
}