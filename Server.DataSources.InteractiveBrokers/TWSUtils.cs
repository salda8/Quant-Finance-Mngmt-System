using Common.EntityModels;
using Common.Enums;
using Common.EventArguments;
using Common.Requests;
using Krs.Ats.IBNet;
using System;
using System.Globalization;
using BarSize = Common.Enums.BarSize;

namespace DataSource.InteractiveBrokers
{
    public static class TwsUtils
    {
        public static ErrorArgs ConvertErrorArguments(ErrorEventArgs args)
        {
            return new ErrorArgs((int)args.ErrorCode, args.ErrorMsg);
        }

        /// <summary>
        ///     Check if a request for historical data obeys the duration limits of TWS.
        /// </summary>
        /// <returns>True if the request obeys the limits, false otherwise.</returns>
        public static bool RequestObeysLimits(HistoricalDataRequest request)
        {
            //The limitations are laid out here: https://www.interactivebrokers.com/en/software/api/apiguide/tables/historical_data_limitations.htm
            var period = request.EndingDate - request.StartingDate;
            var periodSeconds = period.TotalSeconds;

            return periodSeconds < MaxRequestLength(request.Frequency);
        }

        /// <summary>
        ///     Returns the maximum period length of a historical data request, in seconds, depending on the data frequency.
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns>Maximum allowed length in </returns>
        public static int MaxRequestLength(BarSize frequency)
        {
            //The limitations are laid out here: https://www.interactivebrokers.com/en/software/api/apiguide/tables/historical_data_limitations.htm
            if (frequency <= BarSize.OneSecond) return 1800;
            if (frequency <= BarSize.FiveSeconds) return 7200;
            if (frequency <= BarSize.FifteenSeconds) return 14400;
            if (frequency <= BarSize.ThirtySeconds) return 24 * 3600;
            if (frequency <= BarSize.OneMinute) return 2 * 24 * 3600;
            if (frequency <= BarSize.ThirtyMinutes) return 7 * 24 * 3600;
            if (frequency <= BarSize.OneHour) return 29 * 24 * 3600;
            return 365 * 24 * 3600;
        }

        public static OHLCBar HistoricalDataEventArgsToOHLCBar(Krs.Ats.IBNet.HistoricalDataEventArgs e)
        {
            var bar = new OHLCBar
            {
                DateTimeOpen = e.Date,
                Open = e.Open,
                High = e.High,
                Low = e.Low,
                Close = e.Close,
                Volume = e.Volume
            };
            return bar;
        }

        public static string TimespanToDurationString(TimeSpan t, BarSize minFreq)
        {
            //   duration:
            //     This is the time span the request will cover, and is specified using the
            //     format: , i.e., 1 D, where valid units are: S (seconds) D (days) W (weeks)
            //     M (months) Y (years) If no unit is specified, seconds are used. "years" is
            //     currently limited to one.
            if (minFreq > BarSize.OneMonth)
                return Math.Ceiling(Math.Max(1, t.TotalDays / 365)).ToString("0") + " Y";
            if (minFreq >= BarSize.OneMonth)
                return Math.Ceiling(Math.Max(1, t.TotalDays / 29)).ToString("0") + " M";
            if (minFreq >= BarSize.OneWeek)
                return Math.Ceiling(Math.Max(1, t.TotalDays / 7)).ToString("0") + " W";
            if (minFreq >= BarSize.OneDay || t.TotalSeconds > 86400)
            {
                if (t.TotalDays > 14)
                {
                    //This is a ridiculous hack made necessary by the incredibly bad TWS API
                    //For longer periods, if we specify the period as a # of days, the request is rejected!
                    //so instead we do it as the number of weeks and everything is A-OK
                    return Math.Ceiling(t.TotalDays / 7).ToString("0") + " W";
                }
                return Math.Ceiling(Math.Max(1, t.TotalDays)).ToString("0") + " D";
            }

            return Math.Abs(Math.Ceiling(t.TotalSeconds)).ToString("0") + " S";
        }

        public static Krs.Ats.IBNet.BarSize BarSizeConverter(BarSize freq)
        {
            switch (freq)
            {
                case BarSize.Tick:
                    throw new Exception("Bar size conversion impossible, TWS does not suppor tick BarSize");
                case BarSize.OneSecond:
                    return Krs.Ats.IBNet.BarSize.OneSecond;

                case BarSize.FiveSeconds:
                    return Krs.Ats.IBNet.BarSize.FiveSeconds;

                case BarSize.FifteenSeconds:
                    return Krs.Ats.IBNet.BarSize.FifteenSeconds;

                case BarSize.ThirtySeconds:
                    return Krs.Ats.IBNet.BarSize.ThirtySeconds;

                case BarSize.OneMinute:
                    return Krs.Ats.IBNet.BarSize.OneMinute;

                case BarSize.TwoMinutes:
                    return Krs.Ats.IBNet.BarSize.TwoMinutes;

                case BarSize.FiveMinutes:
                    return Krs.Ats.IBNet.BarSize.FiveMinutes;

                case BarSize.FifteenMinutes:
                    return Krs.Ats.IBNet.BarSize.FifteenMinutes;

                case BarSize.ThirtyMinutes:
                    return Krs.Ats.IBNet.BarSize.ThirtyMinutes;

                case BarSize.OneHour:
                    return Krs.Ats.IBNet.BarSize.OneHour;

                case BarSize.OneDay:
                    return Krs.Ats.IBNet.BarSize.OneDay;

                case BarSize.OneWeek:
                    return Krs.Ats.IBNet.BarSize.OneWeek;

                case BarSize.OneMonth:
                    return Krs.Ats.IBNet.BarSize.OneMonth;

                case BarSize.OneQuarter:
                    throw new Exception("Bar size conversion impossible, TWS does not suppor quarter BarSize.");
                case BarSize.OneYear:
                    return Krs.Ats.IBNet.BarSize.OneYear;

                default:
                    return Krs.Ats.IBNet.BarSize.OneDay;
            }
        }

        public static BarSize BarSizeConverter(Krs.Ats.IBNet.BarSize freq)
        {
            if (freq == Krs.Ats.IBNet.BarSize.OneYear) return BarSize.OneYear;
            return (BarSize)(int)freq;
        }

        public static SecurityType SecurityTypeConverter(InstrumentType type)
        {
            if ((int)type >= 13)
                throw new Exception($"Can not convert InstrumentType {type} to SecurityType");
            return (SecurityType)(int)type;
        }

        public static InstrumentType InstrumentTypeConverter(SecurityType type)
        {
            return (InstrumentType)(int)type;
        }

        public static Instrument ContractDetailsToInstrument(ContractDetails contract)
        {
            var instrument = new Instrument
            {
                Symbol = contract.Summary.LocalSymbol,
                UnderlyingSymbol = contract.Summary.Symbol,
                Name = contract.LongName,
                Type = InstrumentTypeConverter(contract.Summary.SecurityType),
                //Multiplier = contract.Summary.Multiplier == null ? 1 : int.Parse(contract.Summary.Multiplier),
                Expiration =
                  DateTime.ParseExact(contract.Summary.Expiry, "yyyyMMdd", CultureInfo.InvariantCulture),
                Currency = contract.Summary.Currency,
                //MinTick = (decimal)contract.MinTick,
                ValidExchanges = contract.ValidExchanges
            };

            //if (instrument.Type == InstrumentType.Future ||
            //    instrument.Type == InstrumentType.FutureOption ||
            //    instrument.Type == InstrumentType.Option)
            //    instrument.TradingClass = contract.Summary.TradingClass;

            return instrument;
        }

        public static Contract InstrumentToContract(Instrument instrument)
        {
            var symbol = instrument.UnderlyingSymbol;

            var expirationString = instrument.Expiration.ToString("yyyyMM", CultureInfo.InvariantCulture);

            var contract = new Contract(
                0,
                symbol,
                SecurityTypeConverter(instrument.Type),
                expirationString,
                0,
                RightType.Undefined,
                string.Empty,
                "",
                instrument.Currency,
                instrument.Symbol,
                string.Empty,
                SecurityIdType.None,
                string.Empty)
            {
                //TradingClass = instrument.TradingClass,
                IncludeExpired = false
            };

            if (instrument.Exchange != null)
            {

                contract.PrimaryExchange = instrument.Exchange.Name;
                contract.Exchange = instrument.Exchange.Name;
            }

            return contract;
        }

        /// <summary>
        ///     Returns RealTimeDataEventArgs derived from IB's  RealTimeBarEventArgs, but not including the symbol
        /// </summary>
        /// <param name="e">RealTimeBarEventArgs</param>
        /// <returns>RealTimeDataEventArgs </returns>
        public static RealTimeDataEventArgs RealTimeDataEventArgsConverter(RealTimeBarEventArgs e)
        {
            return new RealTimeDataEventArgs(
                0,
                e.Time,
                e.Open,
                e.High,
                e.Low,
                e.Close,
                e.Volume,
                e.Wap,
                e.Count,
                0);
        }
    }
}