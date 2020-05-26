using Baco.Windows.SettingsWindow.Options.VideoOptions;
using Baco.Windows.SettingsWindow.Options.VoiceOptions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Baco.Windows.SettingsWindow
{
    class SettingsWindowVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public struct OptionsStruct
        {

            public UserControl UserControl { get; set; }
            public string OptionName { get; set; }

            public OptionsStruct(UserControl userControl, string optionName) : this()
            {
                UserControl = userControl;
                OptionName = optionName;
            }
        }

        public ObservableCollection<OptionsStruct> Options { get; set; }

        public SettingsWindowVM()
        {
            Options = new ObservableCollection<OptionsStruct>
            {
                new OptionsStruct(new VoiceOptionsControl(), "Voice options"),
                new OptionsStruct(new VideoOptionsControl(), "Video options")
            };
        }
    }
}
