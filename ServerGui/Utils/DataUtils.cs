using System;
using Common;
using Common.EntityModels;

namespace ServerGui
{
    public class DataUtils
    {
        public static OHLCBar BarWithRoundedPrices(OHLCBar bar, int decimalPlaces=5)
        {
            var newBar = bar;
           
            newBar.Close = Math.Round(newBar.Close, decimalPlaces);
            newBar.Low = Math.Round(newBar.Low, decimalPlaces);
            newBar.High = Math.Round(newBar.High, decimalPlaces);
            newBar.Open = Math.Round(newBar.Open, decimalPlaces);
            return newBar;
        }
    }
}
