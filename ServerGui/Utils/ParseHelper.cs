using System;
using System.Globalization;
using Common;
using Common.EntityModels;
using LumenWorks.Framework.IO.Csv;
using ServerGui.ViewModels;

namespace ServerGui
{
    public class ParseHelper
    {
        public static decimal ParsePrice(string[] items, int ii)
        {
            decimal price;

            try
            {
                price = Convert.ToDecimal(items[ii].Replace("\"", ""));
            }
            catch (FormatException)
            {
                price = 0;
            }
            return price;
        }
        public static long ParseVolume(string[] items, int ii)
        {
            long price;

            try
            {
                price = Convert.ToInt64(Convert.ToDouble(items[ii].Replace("\"", "")));
            }
            catch (FormatException)
            {
                price = 0;
            }
            return price;
        }
        public static DateTime ParseDateTime(string[] items, int ii, string DateFormatText)
        {
            DateTime price;

            try
            {
                //var result = DateTime.TryParse(, out  price);
                var correctFormat = DateTime.TryParseExact(items[ii].Replace("\"", ""), DateFormatText,
                                                          CultureInfo.InvariantCulture,
                                                          DateTimeStyles.None, out price);
            }
            catch (FormatException)
            {
                price = DateTime.Now;
            }
            return price;
        }

        public static void ParseBarProperty(string columnName, CachedCsvReader csv, int index, OHLCBar bar, string dateFormatText)
        {
            //todo cultureinfo, format provider
            if (columnName.Contains("open"))
            {
                bar.Open = Convert.ToDecimal(csv[index]);
            }
            else if (columnName.Contains("high"))
            {
                bar.High = Convert.ToDecimal(csv[index]);
            }
            else if
                (columnName.Contains("low"))
            {
                bar.Low = Convert.ToDecimal(csv[index]);
            }
            else if (columnName.Contains("close"))
            {
                bar.Close = Convert.ToDecimal(csv[index]);
            }
            else if (columnName.Contains("volume"))
            {
                bar.Volume = Convert.ToInt32(csv[index]);
            }
            else if (columnName.Contains("date"))
            {
                if (DateTime.TryParseExact(csv[index],dateFormatText,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedDateTime))
                {
                    bar.DateTimeClose = parsedDateTime;
                }
                else
                {
                    throw new InvalidDateTimeFormatException("Can't parse date column. Incorrect format");
                }
            }
            
        }
    }
}