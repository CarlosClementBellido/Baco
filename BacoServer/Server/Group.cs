using Baco;
using Baco.ServerObjects;
using BacoServer.CommandLine.Printer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace BacoServer.Server
{
    public class Group : IDisposable
    {
        private const int DEFAULT_SLEEP_MS = 500;
        private const int BUFFER_SIZE_KB = 512;

        private static IFormatter formatter = new BinaryFormatter();

        private List<TcpClientWithInfo> Users { get; set; }

        public int Size { get => Users.Count; }

        public Group()
        {
            Users = new List<TcpClientWithInfo>();
        }

        public void Add(TcpClientWithInfo newUser)
        {
            for (int i = 0; i < Users.Count; i++)
                formatter.Serialize(Users[i].TcpClient.GetStream(), new ServerObject(ServerFlag.NewUserInCall, newUser.Id));

            Users.Add(newUser);
        }

        internal async void FlipFlopMute(int userToFlipFlop, int sender)
        {
            await Task.Run(() =>
            {
                List<int> mutedUsers = Users.FirstOrDefault(f => f.Id == sender).MutedUsers;
                if (!mutedUsers.Contains(userToFlipFlop))
                {
                    mutedUsers.Add(userToFlipFlop);
                    Printer.WriteLine($"{userToFlipFlop} muted by {sender}");
                }
                else
                {
                    mutedUsers.RemoveAll(u => u == userToFlipFlop);
                    Printer.WriteLine($"{userToFlipFlop} unmuted by {sender}");
                }
            });
        }

        internal async void FlipFlopRecivingScreenAsync(int userToFlipFlop, int sender)
        {
            await Task.Run(() =>
            {
                List<int> bannedScreens = Users.FirstOrDefault(f => f.Id == sender).BannedScreens;
                if (!bannedScreens.Contains(userToFlipFlop))
                {
                    bannedScreens.Add(userToFlipFlop);
                    Printer.WriteLine($"{userToFlipFlop} screen banned by {sender}");
                }
                else
                {
                    bannedScreens.RemoveAll(u => u == userToFlipFlop);
                    Printer.WriteLine($"{userToFlipFlop} screen unbanned by {sender}");
                }
            });
        }

        internal void SendToAll(SenderObject senderObject)
        {
            if (senderObject.Flag == SenderFlags.Image)
                Printer.WriteLine("\n\n\tIMAGE\n\n");
            for (int i = 0; i < Users.Count; i++)
                if (Users[i].Id != senderObject.SenderId)
                {
                    try
                    {
                        switch (senderObject.Flag)
                        {
                            case SenderFlags.Image:
                                if (!Users[i].BannedScreens.Contains(senderObject.SenderId))
                                    formatter.Serialize(Users[i].TcpClient.GetStream(), new ServerObject(ServerFlag.SendingData, senderObject));
                                break;
                            case SenderFlags.Voice:
                                if (!Users[i].MutedUsers.Contains(senderObject.SenderId))
                                    formatter.Serialize(Users[i].TcpClient.GetStream(), new ServerObject(ServerFlag.SendingData, senderObject));
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        ExitRoomByIndex(i);
                    }
                }
        }

        internal void SendToAll(ServerObject senderObject)
        {
            for (int i = 0; i < Users.Count; i++)
                try
                {
                    formatter.Serialize(Users[i].TcpClient.GetStream(), senderObject);
                }
                catch (Exception)
                {
                    ExitRoomByIndex(i);
                }
        }

        internal void ExitRoomByIndex(int index)
        {
            int id = Users[index].Id;

            ExitRoom(id);

        }

        internal void ExitRoomById(int id)
        {
            ExitRoom(id);
        }

        private void ExitRoom(int id)
        {
            TcpClientWithInfo user = Users.SingleOrDefault(u => u.Id == id);
            if (user != null)
            {
                if (user.TcpClient.Connected)
                    formatter.Serialize(user.TcpClient.GetStream(), new ServerObject(ServerFlag.EndCall, null));

                Users.Remove(user);

                SendToAll(new ServerObject(ServerFlag.UserLeftRoom, id));
                Printer.WriteLine($"{id} left the room");

                if (Users.Count == 1)
                {
                    formatter.Serialize(Users.FirstOrDefault()?.TcpClient.GetStream(), new ServerObject(ServerFlag.EndCall, null));
                    Users.RemoveAt(0);
                }
            }
        }

        public void Dispose()
        {
            //Users.ForEach(u => formatter.Serialize(u.TcpClient.GetStream(), new ServerObject(ServerFlag.RoomClosed, null)));    // • Normally there will be 0 users; btw it kicks
            //Users.Clear();                                                                                                      // |    and cleans in case of force close
            Printer.WriteLine($"Room closed");
        }

        internal void CallDeclined(int id)
        {
            SendToAll(new ServerObject(ServerFlag.DeclineCall, id));
            if (Users.Count == 1)
                ExitRoomByIndex(0);
        }
    }
}
