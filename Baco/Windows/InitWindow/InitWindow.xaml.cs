using Baco.Debugging;
using Baco.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Baco.Windows.InitWindow
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        public static InitWindow mainWindow { get; private set; }

        public static TabItem hubWindowTabItem;


        public InitWindow()
        {
            InitializeComponent();

            StatusBar.MessageTextBlock = StatusBarTextBlock;

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            mainWindow = this;
            hubWindowTabItem = HubWindowTabItem;

            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingNotificatorCanvas = LoadingNotificator;
            Beautifiers.ToastNotificator.ToastNotificator.ToastNotificatorControl = ToastNotificatorControl;

            /*Client.Friends = new System.Collections.ObjectModel.ObservableCollection<Callables.User>();
            Client.FriendsRetrieved += Client.GetFriends;*/
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void AdjustWindowSize()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void RootWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                    AdjustWindowSize();
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        System.Drawing.Point mousePosition = WindowsUtils.GetMousePosition();
                        Left = mousePosition.X - Width / 2;
                        Top = 0;
                    }
                    DragMove();
                }
        }
    }
}
