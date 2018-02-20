using System;
using System.ComponentModel;

namespace Common.Enums
{
    /// <summary>
    /// Order type.
    /// </summary>
    [Serializable]
    public enum InstrumentType : int
    {
        /// <summary>
        /// Stock
        /// </summary>
        [Description("STK")]
        Stock = 0,

        /// <summary>
        /// Option
        /// </summary>
        [Description("OPT")]
        Option = 1,

        /// <summary>
        /// Future
        /// </summary>
        [Description("FUT")]
        Future = 2,

        /// <summary>
        /// Indice
        /// </summary>
        [Description("IND")]
        Index = 3,

        /// <summary>
        /// FOP = options on futures
        /// </summary>
        [Description("FOP")]
        FutureOption = 4,

        /// <summary>
        /// For Combination Orders - must use combo leg details
        /// </summary>
        [Description("BAG")]
        Bag = 6,

        /// <summary>
        /// CFD
        /// </summary>
        [Description("CFD")]
        CFD = 7,

        /// <summary>
        /// Undefined Security Type
        /// </summary>
        [Description("")]
        Undefined = 12,

        /// <summary>
        /// Backtest result
        /// </summary>
        [Description("Backtest")]
        Backtest = 13
    }
}