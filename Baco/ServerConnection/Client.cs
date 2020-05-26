using Baco.Api;
using Baco.Beautifiers.LoadingNotificator;
using Baco.Debugging;
using Baco.Messaging;
using Baco.Recorder;
using Baco.ServerObjects;
using Baco.Windows.RSSWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Baco.Messaging.Message;
using static Baco.ServerObjects.User;
using Point = System.Windows.Point;

namespace Baco
{
    public class Client : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private const int DEFAULT_BUFFER = 4096;
        private const int MAX_RECONNECTING_ATTEMPTS = 10;
        private const int BUFFER_SIZE = 8192;
        private const int SERVER_PORT = 7854;
        public const string SERVER_ADDRESS = "127.0.0.1";

        public static string serverAddress = SERVER_ADDRESS;
        private static readonly IFormatter formatter = new BinaryFormatter();

        public static int Id { get; set; } = -1;
        public static ObservableCollection<User> Friends { get; set; }
        public static ListBox ListBoxFriends { get; set; }
        public static string Nickname { get; internal set; }
        public static Bitmap ProfilePicture { get; set; } = Resources.Resources._default;
        private static TcpClient senderServer;

        private static int reconnectingAttempts = 0;
        /// <summary>
        /// Sends ServerObject to the server
        /// </summary>
        /// <param name="data">Data to send</param>
        public static async void SendToServer(ServerObject data)
        {
            try
            {
                // If there is no server connection, stablish
                if (senderServer == null || senderServer.Connected == false)
                {
                    LoadingNotificator.LoadingInitialized();
                    try
                    {
                        senderServer = new TcpClient(serverAddress, SERVER_PORT);
                    }
                    catch (SocketException e)
                    {
                        if (reconnectingAttempts >= MAX_RECONNECTING_ATTEMPTS)
                        {
                            StatusBar.SetStatus($"{e.SocketErrorCode.ToString()}\nCannot stablish connection");
                            return;
                        }
                        StatusBar.SetStatus($"{e.SocketErrorCode.ToString()}\nTrying to reconnect in 10 seconds ({++reconnectingAttempts}/{MAX_RECONNECTING_ATTEMPTS} attempts)", 10000);
                        await Task.Delay(10000);
                        SendToServer(data);
                        LoadingNotificator.LoadingFinished();
                        return;
                    }
                    _ = Task.Run(() => ListenServer(senderServer));

                    LoadingNotificator.LoadingFinished();
                }

                lock (senderServer) // To not send too much data at same time
                {
                    if (data.ServerFlag == ServerFlag.Error)
                        MessageBox.Show("Error sendind data to server");

                    if (senderServer.Connected)
                    {
                        formatter.Serialize(senderServer.GetStream(), data);
                        senderServer.GetStream().Flush();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #region SERVER DATA HANDLERS

        public delegate void IncommingCallHandler(int who);
        public static event IncommingCallHandler IncomingCall;
        public delegate void IncommingTextHandler(SenderObjectRelation data, Group group = null);
        public static event IncommingTextHandler IncomingText;
        public delegate void NewUserInCallHandler(int who);
        public static event NewUserInCallHandler NewUserCall;
        public delegate void CallDeclinedHandler(int who);
        public static event CallDeclinedHandler CallDeclined;
        public delegate void CallingToGroupHandler(int who);
        public static event CallingToGroupHandler CallingToGroup;
        public delegate void NewFrameHandler(List<VideoFrame.FrameMapping> frames, Point resolution, int id);
        public static event NewFrameHandler NewFrame;
        public delegate void NewVoiceHandler(byte[] voice, int id);
        public static event NewVoiceHandler NewVoice;
        public delegate void UserLeftRoomHandler(int who);
        public static event UserLeftRoomHandler UserLeftRoom;
        public delegate void EndCallHandler();
        public static event EndCallHandler EndCall;

        #endregion SERVER DATA HANDLERS

        /// <summary>
        /// Data retrieved from server
        /// </summary>
        /// <param name="serverConnection">TCP connection with server</param>
        private static void ListenServer(TcpClient serverConnection)
        {
            serverConnection.ReceiveBufferSize = BUFFER_SIZE;
            serverConnection.SendBufferSize = BUFFER_SIZE;

            NetworkStream networkStream = serverConnection.GetStream();
            while (serverConnection.Connected)
            {
                ServerObject data;
                try
                {
                    data = (ServerObject)formatter.Deserialize(networkStream);
                }
                #region POSSIBLE EXCEPTIONS
                catch (IOException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (SerializationException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (DecoderFallbackException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (OverflowException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (OutOfMemoryException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (NullReferenceException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (IndexOutOfRangeException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (FormatException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (InvalidCastException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (ArgumentOutOfRangeException)
                {
                    ExceptionHandler(networkStream);
                    continue;
                }
                catch (Exception e)
                {
                    ExceptionHandler(networkStream);
                    MessageBox.Show(e.Message, e.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
                #endregion POSSIBLE EXCEPTIONS

                switch (data.ServerFlag)
                {
                    case ServerFlag.ConnectionState:
                        Task.Run(() => SetConnectionState((KeyValuePair<ConnectionState, int>)data.Data));
                        break;
                    case ServerFlag.Call:
                        IncomingCall?.Invoke((int)data.Data);
                        break;
                    case ServerFlag.SendMessage:
                        Task.Run(() => SendMessage((SenderObjectRelation)data.Data));
                        break;
                    case ServerFlag.SendingData:
                        Task.Run(() => SendingData((SenderObject)data.Data));
                        break;
                    case ServerFlag.NewUserInCall:
                        NewUserCall?.Invoke((int)data.Data);
                        break;
                    case ServerFlag.CallingToGroup:
                        CallingToGroup?.Invoke((int)data.Data);
                        break;
                    case ServerFlag.DeclineCall:
                        CallDeclined?.Invoke((int)data.Data);
                        break;
                    case ServerFlag.UserLeftRoom:
                        UserLeftRoom?.Invoke((int)data.Data);
                        break;
                    case ServerFlag.RoomClosed:
                    case ServerFlag.EndCall:
                        EndCall?.Invoke();
                        break;
                    case ServerFlag.ConnectionChecking:
                        Task.Run(() => SendToServer(new ServerObject(ServerFlag.ConnectionChecking, Id)));
                        break;
                    case ServerFlag.ApiConnection:
                        Task.Run(() => CheckApiConnectionFlag((ApiObject)data.Data));
                        break;
                    default:
                        break;
                }
            }
            MessageBox.Show("Disconnected from server");
        }

        private static void ExceptionHandler(NetworkStream networkStream)
        {
            networkStream.Flush();
            byte[] buffer = new byte[DEFAULT_BUFFER];
            while (networkStream.DataAvailable)
                networkStream.ReadAsync(buffer, 0, buffer.Length);
        }

        #region API DATA HANDLERS

        public delegate void HashCheckingHandler(bool success);
        public static event HashCheckingHandler HashCheckingRetrieved;
        public delegate void GetIdHandler(int id);
        public static event GetIdHandler IdRetrieved;
        public delegate void UserPostFeedbackHandler(ApiResponse apiResponse);
        public static event UserPostFeedbackHandler UserPostFeedback;
        public delegate void AcceptPetitionFeedbackHandler(ApiResponse apiResponse);
        public static event AcceptPetitionFeedbackHandler AcceptPetitionFeedback;
        public delegate void SendPetitionFeedbackHandler(ApiResponse apiResponse);
        public static event SendPetitionFeedbackHandler SendPetitionFeedback;
        public delegate void NickCheckingHandler(bool success);
        public static event NickCheckingHandler NickCheckingRetrieved;
        public delegate void MailAvailabilityHandler(bool available);
        public static event MailAvailabilityHandler MailAvailabilityRetrieved;
        public delegate void UsersSerachHandler(List<User> usersFound);
        public static event UsersSerachHandler UsersRetrieved;
        public delegate void RSSSubscriptionsHandler(List<RSSChannel> channels);
        public static event RSSSubscriptionsHandler RSSSubscriptionsRetrieved;
        public delegate void ProfileImageUpdateFeedbackHandler(ApiResponse apiResponse);
        public static event ProfileImageUpdateFeedbackHandler ProfileImageUpdateFeedback;
        public delegate void GetProfilePictureHandler(System.Drawing.Image image);
        public static event GetProfilePictureHandler GetProfilePicture;
        public delegate void GetFriendsHandler(List<User> friends);
        public static event GetFriendsHandler FriendsRetrieved;
        public delegate void GetGroupsHandler(List<Group> groups);
        public static event GetGroupsHandler GroupsRetrieved;
        public delegate void GetPetitionsHandler(List<User> petitioners);
        public static event GetPetitionsHandler PetitionsRetrieved;

        #endregion API DATA HANDLERS

        private static async void CheckApiConnectionFlag(ApiObject data)
        {
            await Task.Delay(1000);
            switch (data.apiFlag)
            {
                case ApiFlag.Error:
                    break;
                case ApiFlag.CheckHash:
                    HashCheckingRetrieved?.Invoke((bool)data.data);
                    break;
                case ApiFlag.GetId:
                    IdRetrieved?.Invoke((int)data.data);
                    break;
                case ApiFlag.PostUser:
                    UserPostFeedback?.Invoke((ApiResponse)data.data);
                    break;
                case ApiFlag.CheckNickAvailability:
                    NickCheckingRetrieved?.Invoke((bool)data.data);
                    break;
                case ApiFlag.CheckMailInUse:
                    MailAvailabilityRetrieved?.Invoke((bool)data.data);
                    break;
                case ApiFlag.FindUsers:
                    UsersRetrieved?.Invoke((List<User>)data.data);
                    break;
                case ApiFlag.GetRSSSubscriptions:
                    RSSSubscriptionsRetrieved?.Invoke((List<RSSChannel>)data.data);
                    break;
                case ApiFlag.UpdateProfilePicture:
                    ProfileImageUpdateFeedback?.Invoke((ApiResponse)data.data);
                    break;
                case ApiFlag.GetProfilePicture:
                    GetProfilePicture?.Invoke((System.Drawing.Image)data.data);
                    break;
                case ApiFlag.GetFriends:
                    FriendsRetrieved?.Invoke((List<User>)data.data);
                    break;
                case ApiFlag.GetGroups:
                    GroupsRetrieved?.Invoke((List<Group>)data.data);
                    break;
                case ApiFlag.GetPetitions:
                    PetitionsRetrieved?.Invoke((List<User>)data.data);
                    break;
                case ApiFlag.AcceptPetition:
                    AcceptPetitionFeedback?.Invoke((ApiResponse)data.data);
                    break;
                case ApiFlag.SendPetition:
                    SendPetitionFeedback?.Invoke((ApiResponse)data.data);
                    break;
            }
        }

        #region API DATA

        public static void GetFriends(List<User> friends)
        {
            Friends = new ObservableCollection<User>(friends);
            ListBoxFriends.Dispatcher.Invoke(new Action(() =>
            {
                ListBoxFriends.ItemsSource = Friends;
            }));
            if (Group.groups != null)
                SendToServer(new ServerObject(ServerFlag.CheckStoredMessages, Id));
        }

        public static async void AcceptPetitionResponse(ApiResponse response)
        {
            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                await Task.Delay(100);
                ApiConn.GetFriends(Id);
            }
        }

        private static void SetConnectionState(KeyValuePair<ConnectionState, int> keyValuePairStateId)
        {
            Friends.FirstOrDefault(f => f.Id == keyValuePairStateId.Value).State = keyValuePairStateId.Key;
            ListBoxFriends.Dispatcher.Invoke(new Action(() =>
            {
                ListBoxFriends.ItemsSource = Friends;
                ListBoxFriends.Items.Refresh();
            }));
        }

        private static void SendMessage(SenderObjectRelation senderObjectRelation)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (senderObjectRelation.Group == null)
                {
                    if (!MessageCollection.ContainsKey(senderObjectRelation.SenderId))
                        MessageCollection.Add(senderObjectRelation.SenderId, new ObservableCollection<Message>());

                    User friend = Friends.SingleOrDefault(f => f.Id == senderObjectRelation.SenderId);

                    MessageCollection[senderObjectRelation.SenderId].Add((Message)senderObjectRelation.Data);
                    IncomingText?.Invoke(senderObjectRelation);
                }
                else
                {
                    Group group = Group.groups.SingleOrDefault(g => g.Id == senderObjectRelation.DestinationId);
                    User friend = Friends.SingleOrDefault(f => f.Id == senderObjectRelation.SenderId);
                    if (friend == null)
                        friend = new User(senderObjectRelation.SenderId);

                    group.Messages.Add(new Message(friend, senderObjectRelation.Data.ToString(), DateTime.Now));
                    IncomingText?.Invoke(senderObjectRelation, group);
                }

            });
        }

        private static void SendingData(SenderObject senderObject)
        {
            switch (senderObject.Flag)
            {
                case SenderFlags.Image:
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        using (MemoryStream ms = new MemoryStream((byte[])senderObject.Data))
                        {
                            object[] objects = (object[])formatter.Deserialize(ms);

                            NewFrame?.Invoke((List<VideoFrame.FrameMapping>)objects[0], (Point)objects[1], senderObject.SenderId);
                        }
                    });
                    break;
                case SenderFlags.Voice:
                    if (senderObject.SenderId != Id)
                        NewVoice?.Invoke((byte[])senderObject.Data, senderObject.SenderId);
                    break;
                default:
                    break;
            }
        }

        #endregion API DATA
    }
}