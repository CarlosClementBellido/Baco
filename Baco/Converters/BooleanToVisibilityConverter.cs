using System;
using System.Windows;
using System.Windows.Data;

namespace Baco.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool)value)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Collapsed;
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
