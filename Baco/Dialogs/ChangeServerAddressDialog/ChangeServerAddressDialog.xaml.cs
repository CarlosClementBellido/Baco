using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Baco.Dialogs.ChangeServerAddressDialog
{
    /// <summary>
    /// Lógica de interacción para ChangeServerAddressDialog.xaml
    /// </summary>
    public partial class ChangeServerAddressDialog : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string ServerName
        {
            get => Client.serverAddress;
            set => Client.serverAddress = value;
        }

        public ChangeServerAddressDialog()
        {
            InitializeComponent();
            DataContext = this;
            ServerName = Client.SERVER_ADDRESS;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Client.serverAddress = Client.SERVER_ADDRESS;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Client.serverAddress = Client.SERVER_ADDRESS;
            DialogResult = false;
            Close();
        }
    }
}
