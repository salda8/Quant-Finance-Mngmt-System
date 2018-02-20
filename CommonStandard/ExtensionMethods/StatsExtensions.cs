using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.ExtensionMethods
{
    public static class StatsExtensions
    {
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public static double StandardDeviation(this IEnumerable<decimal> values)
        {
            decimal avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow((double)(v - avg), 2)));
        }
    }
}