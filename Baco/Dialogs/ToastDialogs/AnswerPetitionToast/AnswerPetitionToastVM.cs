using Baco.Api;
using Baco.Windows.HubWindow;
using System.ComponentModel;

namespace Baco.Dialogs.ToastDialogs.AnswerPetitionToast
{
    class AnswerPetitionToastVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public HubWindowVM hubWindowVM;

        public AnswerPetitionToastVM(HubWindowVM hubWindowVM)
        {
            this.hubWindowVM = hubWindowVM;
        }

        internal void Close()
        {
            hubWindowVM.SelectedPetition = null;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }

        internal void Cancel()
        {
            ApiConn.AcceptPetition(hubWindowVM.SelectedPetition, false);
            hubWindowVM.FriendPetitions.Remove(hubWindowVM.SelectedPetition);
            hubWindowVM.SelectedPetition = null;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }

        internal void Accept()
        {
            ApiConn.AcceptPetition(hubWindowVM.SelectedPetition, true);
            hubWindowVM.FriendPetitions.Remove(hubWindowVM.SelectedPetition);
            hubWindowVM.SelectedPetition = null;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }
    }
}
