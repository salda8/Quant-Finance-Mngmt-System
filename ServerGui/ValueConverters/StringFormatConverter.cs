using System;
using System.Globalization;
using System.Windows.Data;

namespace ServerGui.ValueConverters
{
    internal class StringFormatConverter : IMultiValueConverter
    {
       
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int numberOfDigits = System.Convert.ToInt32(parameter?.ToString());
            
            decimal value = (decimal)values[0];

            return Math.Round(value, numberOfDigits).ToString(culture);
            

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
