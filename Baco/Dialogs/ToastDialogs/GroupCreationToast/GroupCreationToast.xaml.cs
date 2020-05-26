using Baco.Dialogs.GroupCreationDialog;
using System.Windows;
using System.Windows.Controls;

namespace Baco.Dialogs.ToastDialogs.GroupCreationToast
{
    /// <summary>
    /// Lógica de interacción para GroupCreationToast.xaml
    /// </summary>
    public partial class GroupCreationToast : UserControl
    {

        private GroupCreationToastVM groupCreationDialogVM;

        public GroupCreationToast()
        {
            InitializeComponent();
            //Panel.SetZIndex(this, 10);
            groupCreationDialogVM = new GroupCreationToastVM();
            DataContext = groupCreationDialogVM;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            groupCreationDialogVM.SearchBoxTextChanged();
        }

        private void ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            groupCreationDialogVM.SelectedItem();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            groupCreationDialogVM.CreateGroup();
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl.Content = null;
        }
    }
}
