using System;

namespace Baco
{

    [Flags]
    public enum SenderFlags
    {
        Error = 0, Image = 1, Voice = 2
    }

    [Serializable]
    public struct Screen
    {
        public byte[] Bitmap { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Screen(byte[] bitmap, int x, int y)
        {
            Bitmap = bitmap;
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public struct SenderObject
    {
        public SenderFlags Flag { get; set; }
        public int SenderId { get; set; }
        public object Data { get; set; }

        public SenderObject(SenderFlags flag, object data) : this()
        {
            Flag = flag;
            SenderId = Client.Id;
            Data = data;
        }
    }
}
