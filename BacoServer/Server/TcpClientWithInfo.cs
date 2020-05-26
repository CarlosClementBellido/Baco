using System.Collections.Generic;
using System.Net.Sockets;

namespace BacoServer.Server
{
    public class TcpClientWithInfo
    {
        public TcpClient TcpClient { get; set; }
        public int Id { get; set; }
        public List<int> MutedUsers { get; internal set; }
        public List<int> BannedScreens { get; internal set; }

        public TcpClientWithInfo(TcpClient tcpClient, int id)
        {
            TcpClient = tcpClient;
            Id = id;
            MutedUsers = new List<int>();
            BannedScreens = new List<int>();
        }
    }
}
