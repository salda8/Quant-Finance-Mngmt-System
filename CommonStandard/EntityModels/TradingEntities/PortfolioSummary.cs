//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using Common.Interfaces;
//using ProtoBuf;

//namespace Common.EntityModels
//{
//    [ProtoContract]
//    public class PortfolioSummary1 : IEntity
//    {
//        #region Properties
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int ID { get; set; }
//        public virtual Account Account { get; set; }
//        public int AccountID { get; set; }

//        public decimal NetLiquidation { get; set; }
//        public bool LivePosition { get; set; }
//        public decimal OpenPnl { get; set; }
//        public decimal StartEquity { get; set; }
//        public DateTime StartDate { get; set; }
//        public decimal Profit { get; set; }
//        public decimal ProfitPercent { get; set; }
//        public int DaysRunning { get; set; }
//        public decimal DailyPercent { get; set; }
//        public int GatewayPort { get; set; }
//        public virtual Strategy Strategy { get; set; }
//        public int StrategyID { get; set; }


//        #endregion
//    }
//    [ProtoContract]
//    public class PortfolioSummary : IEntity
//    {
//        #region Properties
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        [ProtoMember(1)]
//        public int ID { get; set; }
//        [ProtoMember(2)]
//        public virtual Account Account { get; set; }
//        [ProtoMember(3)]
//        public int AccountID { get; set; }
//        [ProtoMember(4)]
//        public virtual Strategy Strategy { get; set; }
//        [ProtoMember(5)]
//        public int StrategyID { get; set; }
//        [ProtoMember(6)]
//        public decimal Quantity { get; set; }
//        [ProtoMember(7)]
//        public decimal MarketValue { get; set; }
//        [ProtoMember(8)]
//        public decimal MarketPrice { get; set; }
//        [ProtoMember(9)]
//        public decimal AverageCost { get; set; }
//        [ProtoMember(10)]
//        public decimal UnrealizedPnL { get; set; }
//        [ProtoMember(11)]
//        public decimal RealizedPnL { get; set; }


        


//        #endregion
//    }
//}