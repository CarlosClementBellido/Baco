using Baco.Dialogs.GroupCreationDialog;
using System.Windows;
using System.Windows.Controls;

namespace Baco.Windows.HubWindow
{
    /// <summary>
    /// Lógica de interacción para GroupsControl.xaml
    /// </summary>
    public partial class GroupsControl : UserControl
    {
        public GroupsControl()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((HubWindowVM)DataContext).SelectionChanged();
            if (((HubWindowVM)DataContext).SelectedCallable != null)
            {
                RSSWindowControl.Visibility = Visibility.Hidden;
                LocalChatControl.Visibility = Visibility.Visible;
            }
            else
            {
                RSSWindowControl.Visibility = Visibility.Visible;
                LocalChatControl.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GroupCreationDialog groupCreationDialog = new GroupCreationDialog();
            groupCreationDialog.ShowDialog();
        }
    }
}
