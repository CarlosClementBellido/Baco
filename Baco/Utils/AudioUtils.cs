using System.IO;
using System.Media;

namespace Baco.Utils
{
    public static class AudioUtils
    {

        public static void PlaySound(byte[] sound)
        {
            using (MemoryStream ms = new MemoryStream(sound))
            {
                SoundPlayer player = new SoundPlayer(ms);
                player.Play();
            }
        }

    }
}
