using Baco.Recorder;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Baco.Converters
{
    public class ProgressBarAudioForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            float progress = (float)value;
            Brush foreground;

            if (progress < VoiceRecorder.RangeMinAudio)
                foreground = Brushes.Blue;
            else if (progress >= VoiceRecorder.RangeMaxAudio)
                foreground = Brushes.Red;
            else
                foreground = Brushes.Green;

            return foreground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
