using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Baco.Converters
{

    public class NullableBooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool?)value)
            {
                case true:
                    return new SolidColorBrush(Colors.Green);
                case false:
                    return new SolidColorBrush(Colors.Red);
                case null:
                    return new SolidColorBrush(Colors.Transparent);
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
