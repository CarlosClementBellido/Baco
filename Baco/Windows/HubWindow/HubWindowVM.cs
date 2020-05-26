using Baco.Api;
using Baco.Dialogs.AcceptCallDialog;
using Baco.ServerObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static Baco.Client;
using static Baco.Messaging.Message;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using Message = Baco.Messaging.Message;

namespace Baco.Windows.HubWindow
{
    public class HubWindowVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public ICallable SelectedCallable { get; set; }
        public User SelectedPetition { get; set; }
        public User SelectedFoundFriend { get; set; }
        public string NewMessage { get; set; }
        public ObservableCollection<Message> Messages { get; set; }
        public Image ImageViewer { get; set; }

        private string searchString;
        public string SearchString
        {
            get => searchString;
            set
            {
                ApiConn.FindUsers(value);
                searchString = value;
            }
        }
        public List<User> ListBoxFoundFriends { get; set; }
        public ObservableCollection<User> FriendPetitions { get; set; }

        NotifyIcon notifyIcon = new NotifyIcon();
        ICallable LastMessage;

        public HubWindowVM()
        {
            FriendPetitions = new ObservableCollection<User>();

            Client.IncomingCall += IncomingCall;
            Client.IncomingText += IncomingText;
            Client.UsersRetrieved += UsersRetrieved;
            Client.PetitionsRetrieved += PetitionsRetrieved;


            notifyIcon.BalloonTipClosed += (s, e) => notifyIcon.Visible = false;
            notifyIcon.BalloonTipClicked += NotifyIconClick;

            MessageCollection = new Dictionary<int, ObservableCollection<Message>>();

            Messages = new ObservableCollection<Message>();

            SendToServer(new ServerObject(ServerFlag.Connection, Id));

            ApiConn.GetPetitions(Id);
        }

        private void PetitionsRetrieved(List<User> petitions)
        {
            petitions.Where(p => FriendPetitions.FirstOrDefault(f => f.Id == p.Id) == null).ToList().ForEach(u => Application.Current.Dispatcher.Invoke(() => FriendPetitions.Add(u)));
        }

        private void UsersRetrieved(List<User> usersFound)
        {
            ListBoxFoundFriends = usersFound.Where(u => Friends.FirstOrDefault(f => f.Id == u.Id) == null && u.Id != Id).ToList();
        }

        private void IncomingText(SenderObjectRelation data, Group group)
        {
            LastMessage = group == null ? Friends.SingleOrDefault(f => f.Id == data.SenderId) : (ICallable)group;

            LastMessage.Descriptor = $"{LastMessage.Name}: {((Message)data.Data).Content}";

            notifyIcon.Visible = true;
            notifyIcon.Icon = Resources.Resources.baco;

            notifyIcon.ShowBalloonTip(3000, LastMessage.Name ?? "Arnold Schwarzenegger", data.Data.ToString(), ToolTipIcon.None);   // DEPERRENTE ESTO NO FUNCIONA MIERDA JODER MIERDA
        }

        internal void Home()
        {
            SelectedCallable = null;
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            WindowState windowState = InitWindow.InitWindow.mainWindow.WindowState;
            if (windowState == WindowState.Minimized)
                InitWindow.InitWindow.mainWindow.WindowState = WindowState.Normal;

            InitWindow.InitWindow.mainWindow.Topmost = true;

            InitWindow.InitWindow.hubWindowTabItem.IsSelected = true;
            SelectedCallable = LastMessage;

            InitWindow.InitWindow.mainWindow.Topmost = false;
        }

        internal void SelectionChanged()
        {
            Messages = SelectedCallable?.GetChat();
        }

        private void IncomingCall(int who)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                User friend = Friends.FirstOrDefault(f => f.Id == who);
                //SelectedCallable = friend;
                AcceptCallDialog acceptCallDialog = new AcceptCallDialog(friend.Nickname);
                bool? dialogResult = acceptCallDialog.ShowDialog();
                if (dialogResult == true)
                    Task.Run(() => SendToServer(new ServerObject(ServerFlag.CallTo, new int[] { friend.Id, Id })));
                else
                    Task.Run(() => SendToServer(new ServerObject(ServerFlag.DeclineCall, new int[] { friend.Id, Id })));
            });
        }

        internal void SendMessage()
        {
            SelectedCallable.SendMessage(NewMessage);
            NewMessage = "";
        }

        internal void CallUser()
        {
            SelectedCallable.Call();
        }
    }
}
