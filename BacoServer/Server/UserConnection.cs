using Baco.ServerObjects;
using BacoServer.CommandLine.Printer;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Baco.ServerObjects.User;
using static BacoServer.Api.ApiConn;
using static BacoServer.Server.Server;

namespace BacoServer.Server
{
    public class UserConnection
    {
        public int Id { get; set; }
        public Group Group { get; set; }
        public TcpClient Client { get; set; }
        public bool CheckingSent { get; set; }
        public ConnectionState? ConnectionState { get; set; }

        public UserConnection(int id)
        {
            Id = id;
        }

        public UserConnection(int id, Group group, TcpClient client, ConnectionState? connectionState)
        {
            Id = id;
            Group = group;
            Client = client;
            CheckingSent = false;
            ConnectionState = connectionState;
        }

        internal async void CheckConnection()
        {
            if (Ping() == -1 || !Client.Connected)
                Disconnect();
            else
            {
                CheckingSent = true;
                formatter.Serialize(Client.GetStream(), new ServerObject(ServerFlag.ConnectionChecking, null));
                await Task.Delay(5000);
                if (CheckingSent)
                    Disconnect();
            }
        }

        private void Disconnect()
        {
            GetFriendsAndNotifyThem(Id, User.ConnectionState.Disconnected);
            ExitFromGroup();
            connectedUsers.Remove(this);
            Client.Close();
            Printer.WriteLine($"{Id} disconnected");
        }

        internal void ExitFromGroup()
        {
            Group?.ExitRoomById(Id);
            Group = null;
        }

        internal long Ping()
        {
            Ping pingSender = new Ping();

            IPAddress address = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;
            PingReply reply = pingSender.Send(address);

            long pingTime;
            if (reply.Status == IPStatus.Success)
                pingTime = reply.RoundtripTime;
            else
                pingTime = -1;

            return pingTime;
        }

    }

}
