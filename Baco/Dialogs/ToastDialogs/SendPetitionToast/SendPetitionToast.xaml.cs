using Baco.Windows.HubWindow;
using System.Windows;
using System.Windows.Controls;

namespace Baco.Dialogs.ToastDialogs.SendPetitionToast
{
    /// <summary>
    /// Lógica de interacción para SendPetitionToast.xaml
    /// </summary>
    public partial class SendPetitionToast : UserControl
    {

        public SendPetitionToastVM SendPetitionToastVM;

        public SendPetitionToast(HubWindowVM hubWindowVM)
        {
            InitializeComponent();
            SendPetitionToastVM = new SendPetitionToastVM(hubWindowVM);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SendPetitionToastVM.Close();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendPetitionToastVM.Send();
        }
    }
}
