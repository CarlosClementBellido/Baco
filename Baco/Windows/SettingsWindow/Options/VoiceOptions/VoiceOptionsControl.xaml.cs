using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Baco.Windows.SettingsWindow.Options.VoiceOptions
{
    /// <summary>
    /// Lógica de interacción para VoiceOptionsControl.xaml
    /// </summary>
    public partial class VoiceOptionsControl : UserControl
    {

        private VoiceOptionsControlVM voiceOptionsControlVM;

        public VoiceOptionsControl()
        {
            InitializeComponent();
            voiceOptionsControlVM = new VoiceOptionsControlVM();
            DataContext = voiceOptionsControlVM;
        }

        private void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            voiceOptionsControlVM.RestoreDefault((string)((Control)sender).Tag);
        }

        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            voiceOptionsControlVM.StartTest();
        }

        private void StopTestButton_Click(object sender, RoutedEventArgs e)
        {
            voiceOptionsControlVM.StopTest();
        }

        private static readonly Regex regex = new Regex("[^0-9.-]+");
        private void TextBoxNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TextBoxNumeric_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
