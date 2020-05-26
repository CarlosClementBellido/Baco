using System.Windows;
using System.Windows.Controls;

namespace Baco.Dialogs.ToastDialogs.AnswerPetitionToast
{
    /// <summary>
    /// Lógica de interacción para SendPetitionToast.xaml
    /// </summary>
    public partial class AnswerPetitionToast : UserControl
    {
        AnswerPetitionToastVM sendPetitionToastVM;
        public AnswerPetitionToast(Windows.HubWindow.HubWindowVM hubWindowVM)
        {
            InitializeComponent();
            sendPetitionToastVM = new AnswerPetitionToastVM(hubWindowVM);
            DataContext = sendPetitionToastVM;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            sendPetitionToastVM.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            sendPetitionToastVM.Cancel();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            sendPetitionToastVM.Accept();
        }

    }
}
