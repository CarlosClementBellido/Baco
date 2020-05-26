using Baco;
using Baco.Api;
using Baco.ServerObjects;
using BacoServer.CommandLine.Printer;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using static BacoServer.Api.ApiConn;
using static BacoServer.Server.Server;

namespace BacoServer.Server
{
    public static class Actions
    {
        public static void FlipFlopReceivingScreen(ServerObject data)
        {
            Group group = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[0] || u.Id == ((int[])data.Data)[1]).Group;
            group.FlipFlopRecivingScreenAsync(((int[])data.Data)[0], ((int[])data.Data)[1]);
        }

        public static void FlipFlopMute(ServerObject data)
        {
            Group group = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[0] || u.Id == ((int[])data.Data)[1]).Group;
            group.FlipFlopMute(((int[])data.Data)[0], ((int[])data.Data)[1]);
        }

        public static void Connection(ServerObject data, TcpClient tcpClient)
        {
            UserConnection userConnection = connectedUsers.FirstOrDefault(u => u.Id == (int)data.Data);
            if (userConnection == null)
            {
                connectedUsers.Add(new UserConnection((int)data.Data, null, tcpClient, User.ConnectionState.Connected));
                Printer.WriteLine((int)data.Data + " connected");
            }
            else
            {
                if (userConnection.Client.Connected)
                {
                    Printer.WriteLine((int)data.Data + " error: client is already in session", Printer.PrintType.Error);
                    return;
                }
                else
                {
                    userConnection.Client = tcpClient;
                    Printer.WriteLine((int)data.Data + " reconnected");
                }
            }

        }

        public static void CheckStoredMessages(int id)
        {
            if (QueuedMessages.ContainsKey(id))
            {
                QueuedMessages[id].ForEach(m => SendMessage(new ServerObject(ServerFlag.SendMessage, new SenderObjectRelation(m.SenderId, m.DestinationId, m.Data, m.Group)), id));
                QueuedMessages.Remove(id);
            }
        }

        public static void ExitFromGroup(ServerObject data)
        {
            connectedUsers.FirstOrDefault(u => u.Id == (int)data.Data).ExitFromGroup();
        }

        public static void SendMessage(ServerObject data, int? exclusiveMessageId = null)
        {
            SenderObjectRelation senderObjectRelation = (SenderObjectRelation)data.Data;

            if (senderObjectRelation.Group != null)
            {
                Baco.ServerObjects.Group group = groups.SingleOrDefault(g => g.Id == senderObjectRelation.DestinationId);
                senderObjectRelation.Group = group;
                if (exclusiveMessageId == null)
                {
                    group.Users.ForEach(u =>
                    {
                        if (u.Id != senderObjectRelation.SenderId)
                        {
                            UserConnection user = connectedUsers.SingleOrDefault(c => c.Id == u.Id);
                            if (user != null)
                            {
                                formatter.Serialize(user.Client.GetStream(), new ServerObject(ServerFlag.SendMessage, senderObjectRelation));
                                Printer.WriteLine($"Groupal message sent from {senderObjectRelation.SenderId} to {u.Id}:\n\t{senderObjectRelation.Data}");
                            }
                            else
                                AddToQueue(new UserConnection(u.Id), senderObjectRelation);
                        }
                    });
                }
                else
                {
                    UserConnection user = connectedUsers.SingleOrDefault(c => c.Id == exclusiveMessageId);
                    if (user != null)
                    {
                        formatter.Serialize(user.Client.GetStream(), new ServerObject(ServerFlag.SendMessage, senderObjectRelation));
                        Printer.WriteLine($"Groupal message sent from {senderObjectRelation.SenderId} to {exclusiveMessageId} (stored):\n\t{senderObjectRelation.Data}");
                    }
                    else
                        AddToQueue(new UserConnection(exclusiveMessageId.Value), senderObjectRelation);
                }
            }
            else
            {
                UserConnection user = connectedUsers.SingleOrDefault(u => u.Id == senderObjectRelation.DestinationId);
                if (user != null)
                {
                    formatter.Serialize(user.Client.GetStream(), new ServerObject(ServerFlag.SendMessage, senderObjectRelation));
                    Printer.WriteLine($"Message sent from {senderObjectRelation.SenderId} to {user.Id}:\n\t{senderObjectRelation.Data}");
                }
                else
                    AddToQueue(new UserConnection(senderObjectRelation.DestinationId), senderObjectRelation);
            }

            /*for (int i = 0; i < senderObjectRelation.DestinationIds.Length; i++)
            {
                UserConnection user = connectedUsers.FirstOrDefault(u => u.Id == senderObjectRelation.DestinationIds[i]);

                if (user != null)
                {
                    formatter.Serialize(user.Client.GetStream(), new ServerObject(ServerFlag.SendMessage, senderObjectRelation));
                    Printer.WriteLine($"Message sent from {senderObjectRelation.SenderId} to {senderObjectRelation.DestinationIds[i]}:\n\t{senderObjectRelation.Data}");
                }
                else
                {
                    if (!QueuedMessages.ContainsKey(senderObjectRelation.DestinationIds[i]))
                        QueuedMessages[senderObjectRelation.DestinationIds[i]] = new List<Message>();

                    QueuedMessages[senderObjectRelation.DestinationIds[i]].Add((Message)senderObjectRelation.Data);
                    Printer.WriteLine($"Message stored due to unreachable client from {senderObjectRelation.SenderId} to {senderObjectRelation.DestinationIds[i]}:\n\t{senderObjectRelation.Data}");
                }
            }*/

        }

        private static void AddToQueue(UserConnection user, SenderObjectRelation senderObjectRelation)
        {
            if (!QueuedMessages.ContainsKey(user.Id))
                QueuedMessages[user.Id] = new List<SenderObjectRelation>();

            QueuedMessages[user.Id].Add(new SenderObjectRelation(senderObjectRelation.SenderId, senderObjectRelation.DestinationId, senderObjectRelation.Data, senderObjectRelation.Group));
            Printer.WriteLine($"Message stored due to unreachable client from {senderObjectRelation.SenderId} to {user.Id}:\n\t{senderObjectRelation.Data}");
        }

        public static void CallTo(ServerObject data, TcpClient tcpClient)
        {
            UserConnection callingTo = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[0]);   // 0: Connect to; 1: Id
            if (callingTo != null)
            {
                if (callingTo.Group == null)
                {
                    Group newGroup = new Group();
                    connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[1]).Group = newGroup;

                    newGroup.Add(new TcpClientWithInfo(tcpClient, ((int[])data.Data)[1]));
                    formatter.Serialize(callingTo.Client.GetStream(), new ServerObject(ServerFlag.Call, ((int[])data.Data)[1]));
                }
                else
                {
                    if (callingTo.Group.Size == 1)
                    {
                        connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[1]).Group = callingTo.Group;
                        callingTo.Group.Add(new TcpClientWithInfo(tcpClient, ((int[])data.Data)[1]));
                        formatter.Serialize(callingTo.Client.GetStream(), new ServerObject(ServerFlag.NewUserInCall, ((int[])data.Data)[1]));
                    }
                    else
                        formatter.Serialize(callingTo.Client.GetStream(), new ServerObject(ServerFlag.CallingToGroup, ((int[])data.Data)[1]));

                }
            }
        }

        public static void AddToGroup(ServerObject data)
        {
            UserConnection callingFrom = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[1]);
            UserConnection callingTo = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[0]);
            callingTo.Group = callingFrom.Group;
            callingFrom.Group.Add(new TcpClientWithInfo(callingTo.Client, ((int[])data.Data)[0]));
            formatter.Serialize(callingFrom.Client.GetStream(), new ServerObject(ServerFlag.NewUserInCall, ((int[])data.Data)[0]));
        }

        public static void CallDeclined(ServerObject data)
        {
            UserConnection callingFrom = connectedUsers.FirstOrDefault(u => u.Id == ((int[])data.Data)[0]);   // 0: Connect to; 1: Id
            callingFrom?.Group?.CallDeclined(((int[])data.Data)[1]);
        }

        public static void SendingData(ServerObject data)
        {
            SenderObject senderObject = (SenderObject)data.Data;
            connectedUsers.FirstOrDefault(u => u.Id == senderObject.SenderId).Group?.SendToAll(senderObject);
        }

        public static void ReceivingConnectionCheck(ServerObject data)
        {
            connectedUsers.FirstOrDefault(u => u.Id == (int)data.Data).CheckingSent = false;
        }

        public static void ApiRequest(ApiObject data, TcpClient tcpClient)
        {
            ManageClientRequest(data, tcpClient);
        }
    }
}
