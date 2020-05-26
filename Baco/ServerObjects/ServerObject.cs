using System;
using System.Windows;

namespace Baco.ServerObjects
{

    [Flags]
    public enum ServerFlag : uint
    {
        Error = 0,
        Connection = 1,
        Call = 2 << 1,
        ConnectionState = 2 << 2,
        ExitFromGroup = 2 << 3,
        SendMessage = 2 << 4,
        FlipFlopReceivingScreen = 2 << 5,
        CallTo = 2 << 6,
        SendingData = 2 << 7,
        NewUserInCall = 2 << 8,
        FlipFlopMute = 2 << 9,
        UserLeftRoom = 2 << 10,
        EndCall = 2 << 11,
        ConnectionChecking = 2 << 12,
        RoomClosed = 2 << 13,
        ApiConnection = 2 << 14,
        DeclineCall = 65536,
        AddToGroup = 131072,
        CallingToGroup = 262144,
        CheckStoredMessages = 524288
    }

    [Serializable]
    public struct SenderObjectRelation
    {
        public Group Group { get; set; }
        public int SenderId { get; set; }
        public int DestinationId { get; set; }
        public object Data { get; set; }

        public SenderObjectRelation(int senderId, int destinationId, object data, Group group = null) : this()
        {
            SenderId = senderId;
            DestinationId = destinationId;
            Data = data;
            Group = group;
        }
    }

    [Serializable]
    public struct CallObject
    {
        public byte[] data;
        public int startPos;
        public int lenght;

        public CallObject(byte[] data, int startPos, int lenght)
        {
            this.data = data;
            this.startPos = startPos;
            this.lenght = lenght;
        }
    }

    [Serializable]
    public class ServerObject
    {
        public ServerFlag ServerFlag { get; set; }
        public object Data { get; set; }

        public ServerObject(ServerFlag serverFlag, object data)
        {
            ServerFlag = serverFlag;
            Data = data;
        }

        public bool DataIsCorrect()
        {
            if (ServerFlag == ServerFlag.Error)
            {
                MessageBox.Show("Error while checking ServerObject");
                return false;
            }
            else
                return true;
        }
    }
}

