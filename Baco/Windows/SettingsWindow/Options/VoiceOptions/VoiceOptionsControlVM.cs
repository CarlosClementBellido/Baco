using Baco.Recorder;
using Baco.Recorder.Benchmarks;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static Baco.Recorder.Defaults.VoiceRecorderBenchmarkDefaultValues;
using static Baco.Recorder.Defaults.VoiceRecorderDefaultValues;
using static Baco.Windows.SettingsWindow.Options.VoiceOptions.VoiceOptionsConstants;

namespace Baco.Windows.SettingsWindow.Options.VoiceOptions
{

    class VoiceOptionsControlVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<WaveInCapabilities> VoiceInputDevices { get; set; }
        public VoiceRecorderBenchmark VoiceRecorderBenchmark { get; set; }

        public int CurrentDevice
        {
            get => VoiceRecorder.SelectedDevice;
            set => VoiceRecorder.SelectedDevice = value;
        }

        public float RangeMinAudio
        {
            get => VoiceRecorder.RangeMinAudio * 100;
            set => VoiceRecorder.RangeMinAudio = value / 100;
        }

        public float RangeMaxAudio
        {
            get => VoiceRecorder.RangeMaxAudio * 100;
            set => VoiceRecorder.RangeMaxAudio = value / 100;
        }

        public int BitsPerSample
        {
            get => VoiceRecorder.BitsPerSample;
            set => VoiceRecorder.BitsPerSample = value;
        }

        public int Channels
        {
            get => VoiceRecorder.Channels;
            set => VoiceRecorder.Channels = value;
        }

        public int SampleRate
        {
            get => VoiceRecorder.SampleRate;
            set => VoiceRecorder.SampleRate = value;
        }

        public int BufferMs
        {
            get => VoiceRecorder.BufferMs;
            set => VoiceRecorder.BufferMs = value;
        }

        public int Buffers
        {
            get => VoiceRecorder.Buffers;
            set => VoiceRecorder.Buffers = value;
        }

        internal void RestoreDefault(string tag)
        {
            switch (tag)
            {
                case RESTORE_DEFAULT_RANGE_MAX_AUDIO:
                    RangeMaxAudio = DEFAULT_RANGE_MAX_AUDIO * 100;
                    break;
                case RESTORE_DEFAULT_RANGE_MIN_AUDIO:
                    RangeMinAudio = DEFAULT_RANGE_MIN_AUDIO * 100;
                    break;
                case RESTORE_DEFAULT_BITS_PER_SAMPLE:
                    BitsPerSample = DEFAULT_BITS_PER_SAMPLE;
                    break;
                case RESTORE_DEFAULT_CHANNELS:
                    Channels = DEFAULT_CHANNELS;
                    break;
                case RESTORE_DEFAULT_SAMPLE_RATE:
                    SampleRate = DEFAULT_SAMPLE_RATE;
                    break;
                case RESTORE_DEFAULT_BUFFER_MS:
                    BufferMs = DEFAULT_BUFFER_MS;
                    break;
                case RESTORE_DEFAULT_BUFFERS:
                    Buffers = DEFAULT_BUFFERS;
                    break;
                case RESTORE_DEFAULT_RECORD_TIME_S:
                    VoiceRecorderBenchmark.RecordTime = DEFAULT_RECORD_TIME_S;
                    break;
                case RESTORE_DEFAULT_THREESHOLDS:
                    VoiceRecorderBenchmark.Threesholds = DEFAULT_THREESHOLDS;
                    break;
                case RESTORE_DEFAULT_LISTEN_SIMULTANEOUSLY:
                    VoiceRecorderBenchmark.ListenSimultanously = DEFAULT_LISTEN_SIMULTANEOUSLY;
                    break;
            }
        }

        public VoiceOptionsControlVM()
        {
            VoiceRecorderBenchmark = new VoiceRecorderBenchmark();
            VoiceInputDevices = VoiceRecorder.GetDevices();
        }

        internal void StartTest()
        {
            VoiceRecorderBenchmark.StartTest();
        }

        internal void StopTest()
        {
            VoiceRecorderBenchmark.StopTest();
        }
    }
}
