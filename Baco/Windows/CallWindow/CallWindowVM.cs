using Baco.Dialogs.AcceptCallDialog;
using Baco.Recorder;
using Baco.Recording;
using Baco.ServerObjects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Baco.Utils.ImageUtils;
using Image = System.Windows.Controls.Image;

namespace Baco.Windows.CallWindow
{
    class CallWindowVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;


        private Image ImagePreview;

        public ObservableCollection<Call> FriendsInCall { get; set; }
        public ScreenRecorder ScreenRecorder { get; set; }
        public VoiceRecorder VoiceRecorder { get; set; }
        public bool InCall { get; set; }
        public int Quality { get; set; }

        public CallWindowVM(Image imagePreview)
        {
            ImagePreview = imagePreview;

            VoiceRecorder = new VoiceRecorder();
            ScreenRecorder = new ScreenRecorder();

            FriendsInCall = new ObservableCollection<Call>();

            Client.NewUserCall += NewUserCall;
            Client.CallDeclined += CallDeclined;
            Client.CallingToGroup += CallingToGroup;
            Client.NewVoice += NewVoice;
            Client.NewFrame += NewFrame;
            Client.UserLeftRoom += UserLeftRoom;
            Client.EndCall += EndCall;
        }

        private void CallingToGroup(int who)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                User friend = Client.Friends.FirstOrDefault(f => f.Id == who);
                AcceptCallDialog acceptCallDialog = new AcceptCallDialog(friend.Nickname);
                bool? dialogResult = acceptCallDialog.ShowDialog();
                if (dialogResult == true)
                    Task.Run(() => Client.SendToServer(new ServerObject(ServerFlag.AddToGroup, new int[] { friend.Id, Client.Id })));
            });
        }

        private void CallDeclined(int who)
        {
            MessageBox.Show(who.ToString());
        }

        private void EndCall()
        {
            VoiceRecorder.StopRecording();
            ScreenRecorder.StopRecord();

            Application.Current.Dispatcher.Invoke(delegate
            {
                FriendsInCall.Clear();
                InCall = false;
            });
            Client.SendToServer(new ServerObject(ServerFlag.ExitFromGroup, Client.Id));
        }

        private void UserLeftRoom(int who)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                FriendsInCall.Remove(FriendsInCall.FirstOrDefault(f => f.Friend.Id == who));
            });
        }

        internal void HangUp()
        {
            Client.SendToServer(new ServerObject(ServerFlag.ExitFromGroup, Client.Id));
        }

        internal void FlipFlopReceivingScreen(int friendId)
        {
            Client.SendToServer(new ServerObject(ServerFlag.FlipFlopReceivingScreen, new int[] { friendId, Client.Id }));
        }

        internal void FlipFlopSharingScreen()
        {
            if (ScreenRecorder.Recording)
                ScreenRecorder.StopRecord();
            else
                ScreenRecorder.StartRecord(ImagePreview, 1);
        }

        public void FlipFlopMute(int friendId)
        {
            Client.SendToServer(new ServerObject(ServerFlag.FlipFlopMute, new int[] { friendId, Client.Id }));
        }

        private void NewVoice(byte[] voice, int id)
        {
            Call auxFriend = FriendsInCall.FirstOrDefault(f => f.Friend.Id == id);
            if (auxFriend != null)
                auxFriend.AddToQueue(voice);
            else
                NewUserCall(id);
        }

        private void NewFrame(List<VideoFrame.FrameMapping> frames, System.Windows.Point resolution, int id)
        {
            Task.Run(() =>
            {
                Call auxFriend = FriendsInCall.FirstOrDefault(f => f.Friend.Id == id);
                if (auxFriend == null)
                    NewUserCall(id);
                if (auxFriend.Resolution == null)
                    auxFriend.SetResolution(resolution);

                auxFriend.Render(frames);
            });
        }

        private void NewUserCall(int who)
        {
            if (!VoiceRecorder.Recording)
            {
                VoiceRecorder.StartRecording();
                InCall = true;
            }

            if (FriendsInCall.FirstOrDefault(f => f.Friend.Id == who) == null)
                Application.Current.Dispatcher.Invoke(delegate
                {
                    User newFriend = Client.Friends.FirstOrDefault(f => f.Id == who);
                    if (newFriend == null)
                        newFriend = new User(who, "Unknown friend", null);
                    Call newFriendCall = new Call(newFriend);
                    if (!FriendsInCall.Contains(newFriendCall))
                        FriendsInCall.Add(newFriendCall);


                    using (MemoryStream ms = new MemoryStream())
                    {
                        Resources.Resources.welcomeuser.CopyTo(ms);
                        Client.SendToServer(new ServerObject(ServerFlag.SendingData, new SenderObject(SenderFlags.Voice, ms.ToArray())));
                    }


                    newFriendCall.Frame = ImageSourceFromBitmap((Bitmap)newFriend.Picture);

                });
        }

    }
}
