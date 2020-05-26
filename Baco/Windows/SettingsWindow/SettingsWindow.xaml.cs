using System.Windows.Controls;

namespace Baco.Windows.SettingsWindow
{
    /// <summary>
    /// Lógica de interacción para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : UserControl
    {
        SettingsWindowVM settingsWindowVM;

        public SettingsWindow()
        {
            InitializeComponent();
            settingsWindowVM = new SettingsWindowVM();
            DataContext = settingsWindowVM;
        }
    }
}
