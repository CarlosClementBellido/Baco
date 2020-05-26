using Baco.Api;
using Baco.Dialogs.ToastDialogs.AnswerPetitionToast;
using Baco.Dialogs.ToastDialogs.GroupCreationToast;
using Baco.Dialogs.ToastDialogs.SendPetitionToast;
using Baco.ServerObjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Baco.Windows.HubWindow
{
    /// <summary>
    /// Lógica de interacción para HubWindow.xaml
    /// </summary>
    public partial class HubWindow : UserControl
    {

        private HubWindowVM hubWindowVM;

        public HubWindow()
        {
            InitializeComponent();
            hubWindowVM = new HubWindowVM();
            DataContext = hubWindowVM;

            Client.ListBoxFriends = ListBoxFriends;
            Group.ListBoxGroups = ListBoxGroups;
        }

        private void ListBoxFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            hubWindowVM.SelectionChanged();
            if (hubWindowVM.SelectedCallable is ICallable)
            {
                LocalChatControl.Visibility = Visibility.Visible;
                LocalGroupsControl.Visibility = Visibility.Hidden;

                ICallable aux = hubWindowVM.SelectedCallable;
                switch (hubWindowVM.SelectedCallable)
                {
                    case Group g:
                        ListBoxFriends.UnselectAll();
                        break;
                    case User u:
                        ListBoxGroups.UnselectAll();
                        break;
                }
                hubWindowVM.SelectedCallable = aux;

            }
            else
            {
                LocalChatControl.Visibility = Visibility.Hidden;
                LocalGroupsControl.Visibility = Visibility.Visible;
            }
        }
        private async void RefreshFriends_Button_Click(object sender, RoutedEventArgs e)
        {
            ApiConn.GetFriends(Client.Id);
            await Task.Delay(100);
            ApiConn.GetGroups(Client.Id);
            await Task.Delay(100);
            ApiConn.GetPetitions(Client.Id);
            await Task.Delay(100);
            ApiConn.GetProfilePictureFromApiRest(Client.Nickname);
        }

        private void Button_Click_Home(object sender, RoutedEventArgs e)
        {
            hubWindowVM.Home();
            LocalChatControl.Visibility = Visibility.Hidden;
            LocalGroupsControl.Visibility = Visibility.Visible;
        }

        private void Button_Click_CreateGroup(object sender, RoutedEventArgs e)
        {
            GroupCreationToast groupCreationToast = new GroupCreationToast();

            Beautifiers.ToastNotificator.ToastNotificator.Notificate(groupCreationToast);
        }

        private void ListBoxPetitions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AnswerPetitionToast answerPetitionToast = new AnswerPetitionToast(hubWindowVM);
            Beautifiers.ToastNotificator.ToastNotificator.Notificate(answerPetitionToast);
        }

        private void ListBoxFoundFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SendPetitionToast sendPetitionToast = new SendPetitionToast(hubWindowVM);
            Beautifiers.ToastNotificator.ToastNotificator.Notificate(sendPetitionToast);
        }
    }
}
