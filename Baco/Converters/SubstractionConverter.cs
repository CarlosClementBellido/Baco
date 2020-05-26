using System;
using System.Windows.Data;

namespace Baco.Converters
{
    public class SubstractionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value - 30;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value + 30;
        }
    }
}
