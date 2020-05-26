using System.Windows;
using System.Windows.Controls;

namespace Baco.Dialogs.GroupCreationUserControl
{
    /// <summary>
    /// Lógica de interacción para GroupCreationDialog.xaml
    /// </summary>
    public partial class GroupCreationUserControl : UserControl
    {

        private GroupCreationUserControlVM groupCreationDialogVM;

        public GroupCreationUserControl()
        {
            InitializeComponent();
            groupCreationDialogVM = new GroupCreationUserControlVM();
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

        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            groupCreationDialogVM.CreateGroup();
            //Close();
        }
    }
}
