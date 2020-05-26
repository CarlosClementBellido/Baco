using System.Windows;
using System.Windows.Controls;

namespace Baco.Windows.HubWindow
{
    /// <summary>
    /// Lógica de interacción para ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        public ChatControl()
        {
            InitializeComponent();
        }

        private void Button_Click_Send(object sender, RoutedEventArgs e)
        {
            ((HubWindowVM)DataContext).SendMessage();
        }

        private void Button_Click_Call(object sender, RoutedEventArgs e)
        {
            ((HubWindowVM)DataContext).CallUser();
        }
    }
}
