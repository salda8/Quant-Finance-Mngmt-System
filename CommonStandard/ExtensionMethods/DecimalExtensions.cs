using System;

namespace Common.ExtensionMethods
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Returns the number of decimal places.
        /// </summary>
        public static int CountDecimalPlaces(this decimal value)
        {
            return BitConverter.GetBytes(decimal.GetBits(value)[3])[2]; ;
        }
    }
}
