using Baco.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using static Baco.Client;
using static Baco.Messaging.Message;

namespace Baco.ServerObjects
{

    [Serializable]
    public class User : ICallable, INotifyPropertyChanged
    {

        public enum ConnectionState
        {
            Disconnected, Connected
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("nick")]
        public string Nickname { get; private set; }
        [JsonProperty("passHash")]
        private string PassHash { get; set; } = null;
        [JsonProperty("mail")]
        public string Mail { get; private set; }
        public Image Picture { get; set; } = Resources.Resources._default;
        public ConnectionState? State { get; set; } = ConnectionState.Disconnected;
        public string Name { get => Nickname; set => throw new NotSupportedException(); }

        private string descriptor;
        public string Descriptor { get => descriptor ?? State.ToString(); set => descriptor = value; }

        /// <summary>
        /// JSON creation needs a void constructor
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Primitive user; used to make quick and simple Linq actions
        /// </summary>
        /// <param name="id">User id</param>
        public User(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Friends object; only will be visible their id, nickname and if they are connected 
        /// </summary>
        /// <param name="id">Friend's id</param>
        /// <param name="nickname">Friend's nickname</param>
        /// <param name="connected">Friend's actual status</param>
        public User(int id, string nickname, ConnectionState? connectionState) : this(id)
        {
            Nickname = nickname;
            State = connectionState;
        }

        /// <summary>
        /// To make API-Rest posts
        /// </summary>
        /// <param name="nickname">New nickname to be added</param>
        /// <param name="passHash">Hashed requested password</param>
        /// <param name="mail">Mail to activate account</param>
        public User(string nickname, string passHash, string mail)
        {
            Nickname = nickname;
            PassHash = passHash;
            Mail = mail;
        }

        /// <summary>
        /// Sends a call petition
        /// </summary>
        public void Call()
        {
            Task.Run(() => SendToServer(new ServerObject(ServerFlag.CallTo, new int[] { Id, Client.Id })));
        }

        public void SendMessage(string message)
        {
            MessageCollection[Id].Add(new Message(null, message, DateTime.Now));
            SendToServer(new ServerObject(ServerFlag.SendMessage, new SenderObjectRelation(Client.Id, Id, new Message(new User(Client.Id, Client.Nickname, null), message, DateTime.Now))));
            Descriptor = $"You: {message}";
        }

        /// <summary>
        /// Retrieves the chat with the user
        /// </summary>
        /// <returns>Chat with the user</returns>
        public ObservableCollection<Message> GetChat()
        {
            if (!MessageCollection.ContainsKey(Id))
                MessageCollection.Add(Id, new ObservableCollection<Message>());
            return MessageCollection[Id];
        }

        public override string ToString()
        {
            return "{\n" +
                    $"\t\"id\": {Id},\n" +
                    $"\t\"nick\": \"{Nickname}\",\n" +
                    $"\t\"mail\": \"{Mail}\",\n" +
                    $"\t\"passHash\": \"{PassHash}\"\n" +
                    "}";
        }
    }
}
