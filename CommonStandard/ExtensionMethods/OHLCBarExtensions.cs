using Common.EntityModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using NodaTime;

namespace Common.ExtensionMethods
{
    public static class OHLCBarExtensions
    {

        public static LocalDateTime Date(this OHLCBar bar)
        {
            var dateTimeClose = bar.DateTimeClose;
            return new LocalDateTime(dateTimeClose.Year, dateTimeClose.Month, dateTimeClose.Day, dateTimeClose.Hour, dateTimeClose.Minute, dateTimeClose.Second, dateTimeClose.Millisecond);
        }
       
        /// <summary>
        /// Save a collection of OHLCBars to a file, in CSV format.
        /// </summary>
        public static void ToCSVFile(this IEnumerable<OHLCBar> data, string filePath)
        {
            using (StreamWriter file = new StreamWriter(filePath))
            {
                //write header first
                var headerFields = new List<string>
                {
                    "DateTime Open",
                    "Date",
                    "Time",
                    "Open",
                    "High",
                    "Low",
                    "Close",
                    "Volume",
                    "Open Interest",
                    "Dividend",
                    "Split",
                };
                string header = string.Join(",", headerFields);
                file.WriteLine(header);

                foreach (OHLCBar bar in data)
                {
                    file.WriteLine("{7},{0:yyyy-MM-dd},{1:HH:mm:ss.fff},{2},{3},{4},{5},{6},{7},",
                        bar.Date(),
                        bar.Date(),
                        bar.Open,
                        bar.High,
                        bar.Low,
                        bar.Close,
                        bar.Volume,
                       bar.DateTimeOpen);
                }
            }
        }
    }
}