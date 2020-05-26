using Baco.Recorder;
using Baco.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static Baco.Utils.ImageUtils;

using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace Baco.ServerObjects
{
    /// <summary>
    /// Represents a user in a call
    /// </summary>
    public class Call : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private System.Windows.Forms.Timer timer;
        private int currentFPSCount = 0;
        private float currentBPSCount = 0;

        public User Friend { get; set; }
        public ImageSource Frame { get; set; }
        private Queue<byte[]> VoiceQueue { get; set; }
        public bool PlayingVoice { get; set; }
        public Point? Resolution { get; private set; }
        public int FPSRetrieved { get; set; }
        public int KbPSRetrieved { get; set; }
        public bool ShowDebug { get; set; }

        public Call(User friend)
        {
            Frame = new BitmapImage();
            Resolution = null;
            Friend = friend;
            PlayingVoice = false;
            VoiceQueue = new Queue<byte[]>();
            ShowDebug = true;
            FPSRetrieved = 0;
            timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            timer.Tick += FPSChecking;
            timer.Start();
        }

        private void FPSChecking(object sender, EventArgs e)
        {
            FPSRetrieved = currentFPSCount;
            KbPSRetrieved = (int)currentBPSCount / 1000;
            currentFPSCount = 0;
            currentBPSCount = 0;
        }

        /// <summary>
        /// Sets the video resolution given
        /// </summary>
        /// <param name="resolution">Resolution of the user video</param>
        public void SetResolution(Point resolution)
        {
            Resolution = resolution;
            int width = (int)Resolution.Value.X;
            int height = (int)Resolution.Value.Y;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>
            {
                Colors.Red,
                Colors.Blue,
                Colors.Green
            };
            BitmapPalette myPalette = new BitmapPalette(colors);

            Frame.Dispatcher.Invoke(() => Frame = BitmapSource.Create(width, height, 96, 96, PixelFormats.Indexed1, myPalette, pixels, stride));
        }

        /// <summary>
        /// Sets new frame to visible
        /// </summary>
        /// <param name="FrameStream">New image stream</param>
        private void SetFrame(Stream FrameStream)
        {
            currentFPSCount++;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = FrameStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            Frame = bitmapImage;
        }

        Image previousFrame = null;
        bool newFrame = false;
        /// <summary>
        /// Renders all given blocks into last known frame
        /// </summary>
        /// <param name="Frames">New blocks</param>
        public void Render(List<VideoFrame.FrameMapping> Frames)
        {
            newFrame = true;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                newFrame = false;
                Image finalImage = BitmapSourceToBitmap(Frame as BitmapSource);
                ImageConverter ic = new ImageConverter();

                Random rnd = new Random();
                Color color = Color.FromArgb(rnd.Next(0, 255 + 1), rnd.Next(0, 255 + 1), rnd.Next(0, 255 + 1));

                using (Graphics graphics = Graphics.FromImage(finalImage))
                using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(graphics, new Rectangle(0, 0, (int)Resolution.Value.X, (int)Resolution.Value.Y)))
                using (MemoryStream ms = new MemoryStream())
                {
                    bufferedGraphics.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    bufferedGraphics.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                    if (previousFrame != null)
                        bufferedGraphics.Graphics.DrawImage(previousFrame, new Rectangle(0, 0, (int)Resolution.Value.X, (int)Resolution.Value.Y));

                    foreach (VideoFrame.FrameMapping frame in Frames)
                    {
                        /*if (newFrame)   // Screen gets corrupted when too much frames retrieved
                            break;*/
                        using (MemoryStream ms2 = new MemoryStream(frame.Frame))
                        {
                            currentBPSCount += ms2.Length;
                            bufferedGraphics.Graphics.DrawImage(Image.FromStream(ms2), frame.Portion.X, frame.Portion.Y);
                        }
                        if (ShowDebug)
                        {
                            System.Drawing.Pen pen = new System.Drawing.Pen(color)
                            {
                                Width = 2
                            };
                            bufferedGraphics.Graphics.DrawRectangle(pen, frame.Portion.X, frame.Portion.Y, frame.Portion.Width, frame.Portion.Height);
                        }
                    }
                    bufferedGraphics.Render();

                    finalImage.Save(ms, ImageFormat.Png);
                    previousFrame = Image.FromStream(ms);
                    SetFrame(ms);
                }
            }));
        }

        /// <summary>
        /// Adds to queue new audio data
        /// </summary>
        /// <param name="newVoice">Audio to be played</param>
        public void AddToQueue(byte[] newVoice)
        {
            currentBPSCount += newVoice.Length;
            VoiceQueue.Enqueue(newVoice);
            if (!PlayingVoice)
                Task.Run(() => PlayVoiceAsync());
        }

        private async void PlayVoiceAsync()
        {
            PlayingVoice = true;
            while (VoiceQueue.Count > 0)
            {
                await Task.Run(() =>
                {
                    AudioUtils.PlaySound(VoiceQueue.Dequeue());

                    if (VoiceQueue.Count > 1)
                        VoiceQueue.Clear();
                });
            }
            PlayingVoice = false;
        }
    }
}
