using Baco.ServerObjects;
using Baco.Utils;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Baco.Recorder
{
    /// <summary>
    /// Captures the sound from the selected input devide
    /// </summary>
    public class VoiceRecorder : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public static float RangeMinAudio { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_RANGE_MIN_AUDIO;
        public static float RangeMaxAudio { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_RANGE_MAX_AUDIO;
        public static int SampleRate { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_SAMPLE_RATE;
        public static int BitsPerSample { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_BITS_PER_SAMPLE;
        public static int Channels { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_CHANNELS;
        public static int BufferMs { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_BUFFER_MS;
        public static int Buffers { get; set; } = Defaults.VoiceRecorderDefaultValues.DEFAULT_BUFFERS;
        public static bool Threesholds { get; set; } = Defaults.VoiceRecorderBenchmarkDefaultValues.DEFAULT_THREESHOLDS;

        public static int SelectedDevice = 0;

        private bool testing;

        private WaveFormat waveFormat;

        WaveInEvent waveIn;

        public bool Recording { get; private set; }
        public float CurrentVolume { get; set; }

        public VoiceRecorder(bool testing = false)
        {
            this.testing = testing;
        }

        /// <summary>
        /// Finds all input devices in system
        /// </summary>
        /// <returns>Input devices in system</returns>
        public static ObservableCollection<WaveInCapabilities> GetDevices()
        {
            ObservableCollection<WaveInCapabilities> devices = new ObservableCollection<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
                devices.Add(WaveIn.GetCapabilities(i));
            return devices;
        }

        /// <summary>
        /// Starts listening to the input device
        /// </summary>
        public void StartRecording()
        {
            waveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels);
            waveIn = new WaveInEvent
            {
                BufferMilliseconds = BufferMs,
                DeviceNumber = SelectedDevice,
                WaveFormat = waveFormat,
                NumberOfBuffers = Buffers
            };
            waveIn.DataAvailable += DataAvailable;
            waveIn.StartRecording();
            Recording = true;
        }

        /// <summary>
        /// Stops listening to the input device
        /// </summary>
        public void StopRecording()
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            Recording = false;
        }

        public async void RestartRecording()
        {
            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingInitialized();
            StopRecording();
            await Task.Delay(100);
            StartRecording();
            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingFinished();
        }

        /// <summary>
        /// Buffer is completed. It's sent through server or to be listened in system
        /// </summary>
        /// <param name="sender"> - </param>
        /// <param name="e"> - </param>
        private void DataAvailable(object sender, WaveInEventArgs e)
        {
            if (RangePassed(e))
                Task.Run(() =>
                {
                    if (waveIn.DeviceNumber != SelectedDevice)
                    {
                        RestartRecording();
                        return;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (WaveFileWriter writer = new WaveFileWriter(ms, waveFormat))
                        {
                            writer.Write(e.Buffer, 0, e.BytesRecorded);
                        }
                        if (!testing)
                            Client.SendToServer(new ServerObject(ServerFlag.SendingData, new SenderObject(SenderFlags.Voice, ms.ToArray())));
                        else
                            AudioUtils.PlaySound(ms.ToArray());
                    }
                });
        }

        /// <summary>
        /// Checks if volume is too low or too high to be sent
        /// </summary>
        /// <param name="args">WaveInStream event return</param>
        /// <returns>Sound is correct</returns>
        private bool RangePassed(WaveInEventArgs args)
        {
            float max = 0;

            for (int index = 0; index < args.BytesRecorded; index += 2)
            {
                short sample = (short)((args.Buffer[index + 1] << 8) | args.Buffer[index + 0]);
                
                float sample32 = sample / 32768f; // To floating point

                if (sample32 < 0) // Absolute value 
                    sample32 = -sample32;
                
                if (sample32 > max) // Check this the max value
                    max = sample32;
            }

            CurrentVolume = max;
            return Threesholds && max > RangeMinAudio && max < RangeMaxAudio;
        }

    }
}
