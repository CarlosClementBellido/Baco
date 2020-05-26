using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestMessaging.Recording;
using TestMessaging.ServerConnection;

namespace TestMessaging.Windows
{
    /// <summary>
    /// Lógica de interacción para MainApplication.xaml
    /// </summary>
    public partial class MainApplication : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> Messages { get; set; }
        public int ConnectTo { get; set; }

        public MainApplication()
        {
            InitializeComponent();

            Client.SendToServer(new ServerObject(ServerFlag.Connection, Client.Id)); 
            Thread.Sleep(1000);

            Client.IncomingCall += IncomingCall;

            DataContext = this;

            Messages = new ObservableCollection<string>();
            ListBoxMessages.ItemsSource = Messages;
        }

        public void IncomingCall(int who)
        {
            ConnectTo = who;
            //Task.Run(() => Client.StartClientReceiver(ListBoxMessages, ImageViewer, new int[] { ConnectTo, Client.Id }));
        }


        private void Button_Click_SendText(object sender, RoutedEventArgs e)
        {
            Client.Send(new SenderObject(SenderFlags.Text, TextBoxMessage.Text), new int[] { ConnectTo, Client.Id });
        }

        private void Button_Click_StartReceiver(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => Client.StartClientReceiver(ListBoxMessages, ImageViewer, new int[] { ConnectTo, Client.Id }));
        }

        private ScreenRecorder screenRecorder;

        private void Button_Click_SendVideo(object sender, RoutedEventArgs e)
        {
            screenRecorder = new ScreenRecorder();
            //screenRecorder.StartRecord(30);
        }

        private void Button_Click_StopVideo(object sender, RoutedEventArgs e)
        {
            if(screenRecorder != null)
                screenRecorder.StopRecord();
        }

        private void ImageViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FullScreenImageViewer fullScreenImageViewer = new FullScreenImageViewer();
            fullScreenImageViewer.Closing += ClosingImageViewer;
            fullScreenImageViewer.Show();
        }

        private void ClosingImageViewer(object sender, CancelEventArgs e)
        {
            //Client.ImageViewer = ImageViewer;
        }

        private void Button_Click_AddQuality(object sender, RoutedEventArgs e)
        {
            if (screenRecorder != null)
            {
                screenRecorder.SetQuality(screenRecorder.Quality + 1);
                screenRecorder.FPS = 1000 / (int)screenRecorder.Quality;
            }
        }

        private void Button_Click_ReduceQuality(object sender, RoutedEventArgs e)
        {
            if (screenRecorder != null)
            {
                screenRecorder.SetQuality(screenRecorder.Quality - 1);
                screenRecorder.FPS = 1000 / (int)screenRecorder.Quality;
            }
        }
    }
}
