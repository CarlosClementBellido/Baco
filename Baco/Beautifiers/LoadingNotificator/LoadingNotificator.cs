using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Baco.Beautifiers.LoadingNotificator
{
    /// <summary>
    /// Controls the loading notification
    /// </summary>
    public static class LoadingNotificator
    {
        public static Canvas LoadingNotificatorCanvas { get; set; }
        public static int LoadingPetitions { get; set; }

        /// <summary>
        /// When something is going to take time to be completed
        /// </summary>
        public static void LoadingInitialized()
        {
            if (++LoadingPetitions == 1)
                Task.Run(() => StartAnimation());
        }

        /// <summary>
        /// Something has finished the loading
        /// </summary>
        public static void LoadingFinished()
        {
            if (LoadingPetitions > 0)
                LoadingPetitions--;
        }

        /// <summary>
        /// Makes the loading icon spin and change color
        /// </summary>
        private static void StartAnimation()
        {
            for (int i = 0; LoadingPetitions > 0; i++)
                LoadingNotificatorCanvas.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        LoadingNotificatorCanvas.Background = new SolidColorBrush(Color.FromRgb((byte)(i - 255), (byte)(i - 128), (byte)i));
                        RotateTransform rotateTransform = new RotateTransform(i, LoadingNotificatorCanvas.Width / 2, LoadingNotificatorCanvas.Height / 2);
                        LoadingNotificatorCanvas.RenderTransform = rotateTransform;
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                });
        }

    }
}
