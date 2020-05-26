using System.ComponentModel;
using static Baco.Recorder.Defaults.VoiceRecorderBenchmarkDefaultValues;

namespace Baco.Recorder.Benchmarks
{
    /// <summary>
    /// For voice tests inside options menu
    /// </summary>
    public class VoiceRecorderBenchmark : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public int RecordTime { get; set; } = DEFAULT_RECORD_TIME_S;
        public bool ListenSimultanously { get; set; } = DEFAULT_LISTEN_SIMULTANEOUSLY;
        public bool Threesholds { get; set; } = DEFAULT_THREESHOLDS;
        public VoiceRecorder VoiceRecorder { get; private set; }
        public bool Testing { get; set; }

        public VoiceRecorderBenchmark()
        {
            Testing = false;
        }

        public void StartTest()
        {
            VoiceRecorder = new VoiceRecorder(true);
            VoiceRecorder.Threesholds = Threesholds;
            VoiceRecorder.StartRecording();
            Testing = true;
        }

        internal void StopTest()
        {
            VoiceRecorder.StopRecording();
            Testing = false;
        }
    }
}
