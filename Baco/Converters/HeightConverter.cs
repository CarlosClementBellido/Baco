using System;
using System.Windows.Data;

namespace Baco.Converters
{
    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] parameters = parameter.ToString().Split(';');
            return (bool)value ? parameters[0] : parameters[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
