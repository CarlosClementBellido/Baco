using Baco.Recorder;
using Baco.ServerObjects;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Device = SharpDX.Direct3D11.Device;
using Image = System.Windows.Controls.Image;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Point = System.Windows.Point;
using static Baco.Utils.ImageUtils;

namespace Baco.Recording
{
    /// <summary>
    /// Captures the screen and outputs to server
    /// </summary>
    public class ScreenRecorder : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ImageChanged { get; private set; }
        private int quality;
        public int Quality
        {
            get => quality;
            set => SetQuality(value);
        }
        public bool Recording { get; private set; }
        public int FPS { get; set; }
        public int RealFPS { get; set; }

        private int width;
        private int height;
        private int numAdapter;
        private int numOutput;
        private Factory1 factory;
        private Adapter1 adapter;
        private Device device;
        private Output output;
        private Output1 output1;
        private Texture2DDescription texture2DDescription;
        private Texture2D screenTexture;
        // Specifies the dimensions of the surface that contains the desktop image
        private OutputDuplication duplicatedOutput;
        // Where the images will be taken
        private SharpDX.DXGI.Resource screenResource;
        // Control of the sharing screen preview
        private Image ImagePreview;

        // Represents the image quality
        private readonly Encoder encoder;
        // Determines the quality to the captured frame
        private readonly EncoderParameters encoderParameters;
        // Codecs of the captured desktop image
        private readonly ImageCodecInfo imageCodecInfo;

        public ScreenRecorder(int numAdapter = 0, int numOutput = 0)
        {
            this.numAdapter = numAdapter;
            this.numOutput = numOutput;
            encoder = Encoder.Quality;
            encoderParameters = new EncoderParameters(1);
            Quality = 30;

            EncoderParameter encoderParameter = new EncoderParameter(encoder, Quality);
            encoderParameters.Param[0] = encoderParameter;

            imageCodecInfo = GetEncoderInfo("image/jpeg");
            InitDX();
        }

        public void SetQuality(int newQuality)
        {
            quality = newQuality;
            EncoderParameter encoderParameter = new EncoderParameter(encoder, Quality);
            encoderParameters.Param[0] = encoderParameter;
        }

        private void InitDX()
        {
            try
            {
                factory = new Factory1();
                adapter = factory.GetAdapter1(numAdapter);
                device = new Device(adapter);
                output = adapter.GetOutput(numOutput);
                output1 = output.QueryInterface<Output1>();

                // Width/Height of desktop to capture
                width = output.Description.DesktopBounds.Left + output.Description.DesktopBounds.Right;
                height = output.Description.DesktopBounds.Top + output.Description.DesktopBounds.Bottom;

                texture2DDescription = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = width,
                    Height = height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                };

                screenTexture = new Texture2D(device, texture2DDescription);
                duplicatedOutput = output1.DuplicateOutput(device);
                screenResource = null;
            }
            catch
            {
                Debug.WriteLine("Error InitDX");
            }
        }

        private void DisposeDX()
        {
            factory.Dispose();
            factory = null;
            adapter.Dispose();
            adapter = null;
            device.Dispose();
            device = null;
            output.Dispose();
            output = null;
            output1.Dispose();
            output1 = null;

            screenTexture.Dispose();
            screenTexture = null;
            duplicatedOutput.Dispose();
            duplicatedOutput = null;
            GC.Collect();
        }

        readonly BinaryFormatter formatter = new BinaryFormatter();
        private async Task GetShotAsync()
        {
            try
            {
                OutputDuplicateFrameInformation duplicateFrameInformation;
                // Try to get duplicated frame within given time
                duplicatedOutput.AcquireNextFrame(5000, out duplicateFrameInformation, out screenResource);

                ImageChanged = duplicateFrameInformation.LastPresentTime == 0;

                // copy resource into memory that can be accessed by the CPU
                using (Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>())
                {
                    device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
                }

                // Get the desktop capture texture
                DataBox mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);
                Rectangle boundsRect = new Rectangle(0, 0, width, height);

                await Task.Run(() =>
                {
                    using (Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        // Copy pixels from screen capture Texture to GDI bitmap
                        BitmapData bitmapData = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        IntPtr sourcePtr = mapSource.DataPointer;
                        IntPtr destinationPtr = bitmapData.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            // Copy a single line 
                            Utilities.CopyMemory(destinationPtr, sourcePtr, width * 4);

                            // Advance pointers
                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                            destinationPtr = IntPtr.Add(destinationPtr, bitmapData.Stride);
                        }

                        // Release source and dest locks
                        bitmap.UnlockBits(bitmapData);

                        device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap.Save(ms, imageCodecInfo, encoderParameters);
                            byte[] bitmapToSend = ms.ToArray();
                            Bitmap bmp;
                            using (MemoryStream msBmp = new MemoryStream(bitmapToSend))
                            {
                                bmp = new Bitmap(msBmp);
                            }

                            ImagePreview.Dispatcher.Invoke(new Action(() =>
                            {
                                ImagePreview.Source = ImageSourceFromBitmap(bmp);
                            }));

                            // Split and check changes
                            List<VideoFrame.FrameMapping> updatedFrames = VideoFrame.UpdateFrameParts(bmp, imageCodecInfo, encoderParameters);

                            if (updatedFrames != null)  // Check if there is any changes
                                using (MemoryStream ms2 = new MemoryStream())
                                {
                                    // Compress updatedFrames into byte[] 
                                    formatter.Serialize(ms2, new object[] { updatedFrames, new Point(width, height) });
                                    Task.Run(() => Client.SendToServer(new ServerObject(ServerFlag.SendingData, new SenderObject(SenderFlags.Image, ms2.ToArray()))));
                                }
                        }
                    }
                });

            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Result.Code)
                {
                    Debug.WriteLine("Error GetShot ACCESS LOST - relaunch in 2s!");
                    Thread.Sleep(2000);
                    DisposeDX();
                    GC.Collect();
                    InitDX();
                }
                else if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    Debug.WriteLine(e.Message);
                    throw;
                }
            }
            finally
            {
                try
                {
                    // Dispose manually
                    if (screenResource != null)
                    {
                        screenResource.Dispose();
                        screenResource = null;
                        duplicatedOutput.ReleaseFrame();
                    }

                    // Force the Garbage Collector to cleanup memory to prevent memory leaks
                    await Task.Factory.StartNew(() => { GC.Collect(); });
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error GetShot finnaly - relaunch in 2s!");
                    Thread.Sleep(2000);
                    DisposeDX();
                    GC.Collect();
                    InitDX();
                }

            }
        }

        private int realFPS = 0;
        /// <summary>
        /// Starts the recording and send to all users in call
        /// </summary>
        /// <param name="imagePreview">Where is going to be the previsualization</param>
        /// <param name="limitFPS">Limit of the sending rate</param>
        public void StartRecord(Image imagePreview, int limitFPS = 0)
        {
            if (!Recording)
            {
                System.Timers.Timer timerRealFPS = new System.Timers.Timer
                {
                    Interval = 1000
                };
                timerRealFPS.Elapsed += RealFrameElapsed;
                timerRealFPS.Start();
                ImagePreview = imagePreview;
                Recording = true;
                FPS = limitFPS;
                Task.Run(async () =>
                {
                    while (Recording)
                    {
                        await GetShotAsync();
                        await Task.Delay(1000 / FPS);
                        realFPS++;
                    }
                });
            }
        }

        /// <summary>
        /// Sets the visible FPS rate
        /// </summary>
        /// <param name="sender"> - </param>
        /// <param name="e"> - </param>
        private void RealFrameElapsed(object sender, ElapsedEventArgs e)
        {
            RealFPS = realFPS;
            realFPS = 0;
        }

        /// <summary>
        /// Stops the reconrding and the data sending of video
        /// </summary>
        public void StopRecord()
        {
            Recording = false;
        }
    }
}
