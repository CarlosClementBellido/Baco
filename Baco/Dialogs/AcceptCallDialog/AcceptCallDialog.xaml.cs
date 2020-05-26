using System.Windows;

namespace Baco.Dialogs.AcceptCallDialog
{
    /// <summary>
    /// Lógica de interacción para AcceptCallDialog.xaml
    /// </summary>
    public partial class AcceptCallDialog : Window
    {

        public string Caller { get; set; }

        public AcceptCallDialog(string caller)
        {
            InitializeComponent();
            Caller = caller;
            DataContext = this;
        }

        private void Button_Click_Answer(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click_Decline(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
