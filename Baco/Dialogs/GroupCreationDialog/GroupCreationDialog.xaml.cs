using System.Windows;
using System.Windows.Controls;

namespace Baco.Dialogs.GroupCreationDialog
{
    /// <summary>
    /// Lógica de interacción para GroupCreationDialog.xaml
    /// </summary>
    public partial class GroupCreationDialog : Window
    {

        private GroupCreationDialogVM groupCreationDialogVM;

        public GroupCreationDialog()
        {
            InitializeComponent();
            groupCreationDialogVM = new GroupCreationDialogVM();
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
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            groupCreationDialogVM.CreateGroup();
            Close();
        }
    }
}
