using Baco.Windows.HubWindow;

namespace Baco.Dialogs.ToastDialogs.SendPetitionToast
{
    public class SendPetitionToastVM
    {

        public HubWindowVM hubWindowVM;

        public SendPetitionToastVM(HubWindowVM hubWindowVM)
        {
            this.hubWindowVM = hubWindowVM;
        }

        internal void Send()
        {
            Api.ApiConn.SendPetition(hubWindowVM.SelectedFoundFriend);
            hubWindowVM.SelectedFoundFriend = null;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }

        internal void Close()
        {
            hubWindowVM.SelectedFoundFriend = null;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }
    }
}
