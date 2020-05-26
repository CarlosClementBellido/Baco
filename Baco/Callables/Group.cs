using Baco.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Baco.Client;

namespace Baco.ServerObjects
{

    [Serializable]
    public class Group : ICallable, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public static ObservableCollection<Group> groups = new ObservableCollection<Group>();
        public static ListBox ListBoxGroups { get; set; }

        public ObservableCollection<Message> Messages = new ObservableCollection<Message>();



        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("users")]
        public List<User> Users { get; set; }


        public System.Drawing.Image Picture { get; set; } = Resources.Resources._default;
        public string Descriptor
        {
            get => string.Join(", ", Users.Where(u => u.Id != Client.Id).Select(u => u.Name));
            set => string.Join(", ", Users.Where(u => u.Id != Client.Id).Select(u => u.Name));
        }

        /// <summary>
        /// JSON creation needs a void constructor
        /// </summary>
        public Group()
        {
        }

        /// <summary>
        /// Primitive group; used to make quick and simple Linq actions
        /// </summary>
        /// <param name="id">Group id</param>
        public Group(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Creates a basic group
        /// </summary>
        /// <param name="users">Users in the group</param>
        /// <param name="groupName">Name given to the group</param>
        public Group(List<User> users, string groupName)
        {
            Users = users;
            Name = groupName;
        }

        /// <summary>
        /// Calls each user of the group
        /// </summary>
        public void Call()
        {
            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingInitialized();

            Users.ForEach(async u =>
            {
                if (u.Id != Id)
                {
                    SendToServer(new ServerObject(ServerFlag.CallTo, new int[] { u.Id, Id }));
                    await Task.Delay(100);
                }
            });

            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingFinished();
        }

        /// <summary>
        /// Sends a message to all users of the group
        /// </summary>
        /// <param name="message">Message</param>
        public void SendMessage(string message)
        {
            Messages.Add(new Message(null, message, DateTime.Now));
            SendToServer(new ServerObject(ServerFlag.SendMessage, new SenderObjectRelation(Client.Id, Id, new Message(new User(Client.Id), message, DateTime.Now), new Group(Id))));
        }

        /// <summary>
        /// Retrieves the chat with the user
        /// </summary>
        /// <returns>Groupal chat</returns>
        public ObservableCollection<Message> GetChat()
        {
            return Messages;
        }

        /// <summary>
        /// Groups are retrieved from the server
        /// </summary>
        /// <param name="groups">Groups retrieved</param>
        public static void GroupsRetrieved(List<Group> groups)
        {
            Group.groups = new ObservableCollection<Group>(groups);
            ListBoxGroups.Dispatcher.Invoke(new Action(() =>
            {
                ListBoxGroups.ItemsSource = Group.groups;
            }));
            if (Friends != null)
                SendToServer(new ServerObject(ServerFlag.CheckStoredMessages, Client.Id));
        }
    }
}
