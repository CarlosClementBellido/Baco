using Baco.Api;
using Baco.Dialogs.ChangeServerAddressDialog;
using Baco.ServerObjects;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Baco.Api.ApiConn;

namespace Baco
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public bool? NickAvailable { get; set; }
        public string Nick { get; set; }
        public bool? MailNotInUse { get; set; }
        public string Mail { get; set; }
        public bool SignInClicked { get; set; }

        private string passwordConfirmation;
        private string password;
        public bool? PasswordOK { get; set; }
        public string Password
        {
            get => password;
            set
            {
                PasswordOK = SignInClicked ? PasswordConfirmation == value : (bool?)null;
                password = value;
            }
        }
        public string PasswordConfirmation
        {
            get => passwordConfirmation;
            set
            {
                PasswordOK = SignInClicked ? Password == value : (bool?)null;
                passwordConfirmation = value;
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            Debugging.StatusBar.MessageTextBlock = TextBlockStatus;

            DataContext = this;

            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingNotificatorCanvas = LoadingNotificator;

            SignInClicked = false;
            NickAvailable = null;
            MailNotInUse = null;
            PasswordOK = null;

            Nick = "nick1";
            Password = "1234";

            Client.IdRetrieved += IdRetrieved;
            Client.HashCheckingRetrieved += HashCheckingRetrieved;
            Client.UserPostFeedback += UserPostFeedback;
            Client.NickCheckingRetrieved += NickCheckingRetrieved;
            Client.MailAvailabilityRetrieved += MailAvailabilityRetrieved;

            Client.GroupsRetrieved += Group.GroupsRetrieved;
            Client.FriendsRetrieved += Client.GetFriends;
            Client.AcceptPetitionFeedback += Client.AcceptPetitionResponse;
        }

        private void MailAvailabilityRetrieved(bool available)
        {
            MailNotInUse = available;
        }

        private void NickCheckingRetrieved(bool success)
        {
            NickAvailable = success;
        }

        private void UserPostFeedback(ApiResponse apiResponse)
        {
            MessageBox.Show(((int)apiResponse.StatusCode >= 200 && (int)apiResponse.StatusCode < 300).ToString(), apiResponse.ReasonPhrase);
        }

        private void IdRetrieved(int id)
        {
            Client.Id = id;
        }

        private async void HashCheckingRetrieved(bool success)
        {
            if (success)
            {
                GetIdFromApiRest(Nick);
                Client.Nickname = Nick;

                do
                    await Task.Delay(25);
                while (Client.Id == -1);    // Wait until the id is retrieved

                await Task.Delay(1000);

                await Application.Current.Dispatcher.Invoke(async delegate
                 {
                     Windows.InitWindow.InitWindow mainWindow = new Windows.InitWindow.InitWindow();

                     await GetInitDataAsync();

                     Close();
                     mainWindow.Show();
                 });
            }
            else
                MessageBox.Show("Bad credentials");
        }

        private async Task GetInitDataAsync()
        {
            await Task.Delay(200);
            Client.Friends = null;
            Group.groups = null;
            await Task.Delay(200);
            GetFriends(Client.Id);
            await Task.Delay(200);
            GetGroups(Client.Id);
            await Task.Delay(200);
        }

        private void LogIn_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!SignInClicked)
                CheckHashFromApiRest(Nick, Password);
            else
            {
                SignInClicked = false;
                NickAvailable = null;
                MailNotInUse = null;
                PasswordOK = null;
            }
        }

        private void SignIn_Button_Click(object sender, RoutedEventArgs routedEventArgs)
        {

            if (SignInClicked)
            {
                if (NickAvailable.Value && MailNotInUse.Value && PasswordOK.Value)
                    ApiRestPostRequest(Nick, Password, Mail);
            }
            else
            {
                SignInClicked = true;
                NickAvailable = false;
                MailNotInUse = false;
                PasswordOK = false;
            }
        }

        private void Nick_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SignInClicked)
                CheckNickAvailability(Nick);
        }

        private void Mail_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SignInClicked)
                CheckMailInUse(Mail);
        }

        private void CloseWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindow_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            { 
                DragMove();
            }
            catch(InvalidOperationException) { }
        }

        private void LoadingNotificator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeServerAddressDialog changeServerAddressDialog = new ChangeServerAddressDialog();
            changeServerAddressDialog.ShowDialog();
            changeServerAddressDialog.Owner = this;
        }
    }
}
