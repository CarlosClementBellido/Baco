using System.Windows;
using System.Windows.Controls;

namespace Baco.Windows.CallWindow
{
    /// <summary>
    /// Lógica de interacción para CallWindow.xaml
    /// </summary>
    public partial class CallWindow : UserControl
    {

        private CallWindowVM callWindowVM;

        public CallWindow()
        {
            InitializeComponent();
            callWindowVM = new CallWindowVM(ImagePreview);
            DataContext = callWindowVM;
        }

        private void Button_Click_FlipFlopMute(object sender, RoutedEventArgs e)
        {
            callWindowVM.FlipFlopMute((int)((Control)sender).Tag);
        }

        private void Button_Click_FlipFlopReceivingScreen(object sender, RoutedEventArgs e)
        {
            callWindowVM.FlipFlopReceivingScreen((int)((Control)sender).Tag);
        }

        private void Button_Click_FlipFlopSharingScreen(object sender, RoutedEventArgs e)
        {
            callWindowVM.FlipFlopSharingScreen();
        }

        private void Button_Click_HangUp(object sender, RoutedEventArgs e)
        {
            callWindowVM.HangUp();
        }
    }
}
