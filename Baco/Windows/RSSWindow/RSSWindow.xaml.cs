using Baco.Api;
using Gecko;
using Gecko.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Xml;

namespace Baco.Windows.RSSWindow
{
    /// <summary>
    /// Lógica de interacción para RSSWindow.xaml
    /// </summary>
    public partial class RSSWindow : UserControl, INotifyPropertyChanged
    {

        public struct Feed
        {
            public Uri Link { get; set; }
            public Uri Image { get; set; }
            public string Title { get; set; }
            public string Categories { get; set; }

            public Feed(Uri link, Uri image, string title, string categories)
            {
                Link = link;
                Image = image;
                Title = title;
                Categories = categories;
            }
        }

        public struct Feeder
        {
            public Uri Image { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public SyndicationFeed SyndicationFeed { get; set; }
            private XmlReader XmlReader { get; set; }

            public Feeder(string url)
            {
                Url = url;
                XmlReader = XmlReader.Create(url);
                SyndicationFeed = SyndicationFeed.Load(XmlReader);
                XmlReader.Close();
                Name = SyndicationFeed.Title.Text;
                Image = SyndicationFeed.ImageUrl;
            }

            public Feeder(string url, XmlReader xmlReader)
            {
                Url = url;
                XmlReader = xmlReader;
                SyndicationFeed = SyndicationFeed.Load(XmlReader);
                XmlReader.Close();
                Name = SyndicationFeed.Title.Text;
                Image = SyndicationFeed.ImageUrl;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Feed> Feeds { get; private set; }
        public ObservableCollection<Feeder> Feeders { get; private set; }
        public bool Loading { get; set; }
        public bool IsFindingChannel { get; set; }
        public bool IsBrowserActive { get; set; }
        public bool IsBrowserNormal { get; set; }
        public Feed SelectedFeed { get; set; }

        private string channelFinder;
        public string ChannelFinder
        {
            get => channelFinder;
            set
            {
                channelFinder = value;
                IsFindingChannel = !string.IsNullOrEmpty(channelFinder);
            }
        }

        private WindowsFormsHost host;
        private GeckoWebBrowser browser;

        public RSSWindow()
        {
            InitializeComponent();
            Client.RSSSubscriptionsRetrieved += RSSSubscriptionsRetrieved;

            DataContext = this;
            ChannelFinder = "";

            Loading = false;
            IsFindingChannel = false;
            IsBrowserNormal = false;

            ApiConn.GetSubscriptionsFromApiRest(Client.Id);
        }

        private void RSSSubscriptionsRetrieved(List<RSSChannel> channels)
        {
            Feeders = new ObservableCollection<Feeder>(channels.Select(c => new Feeder(c.RSS)));
        }

        private string currentUrl = null;
        private void ListBoxRSS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Feed? feed = (Feed)((ListBox)sender).SelectedItem;
            Navigate(SelectedFeed.Link.ToString());
        }

        private void ListBoxRSSFeeders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Feeder? feeder = (Feeder)((ListBox)sender).SelectedItem;
            Feeds = new ObservableCollection<Feed>(feeder.Value.SyndicationFeed.Items.Select(f => new Feed(f.Links[0].Uri, feeder.Value.SyndicationFeed.ImageUrl, f.Title.Text,
                string.Join(", ", f.Categories.Select(c => c.Name)))));
        }

        private void InitializeViewer()
        {
            IsBrowserActive = true;

            if (!Xpcom.IsInitialized)
                Xpcom.Initialize("Firefox");


            Xpcom.EnableProfileMonitoring = false;

            GeckoPreferences.User["browser.cache.memory.enable"] = true; // Cache stuff in RAM if possible
            GeckoPreferences.User["browser.cache.memory.capacity"] = 512; // auto sets the RAM usage to 2GB. Game has a 3GB RAM minimum system requirement
            GeckoPreferences.User["browser.cache.memory.max_entry_size"] = -1; // -1 means no max entry size
            GeckoPreferences.User["Browser.cache.memory.capacity()"] = -1;
            GeckoPreferences.User["Browser.cache.disk.capacity"] = 512;
            GeckoPreferences.User["browser.xul.error_pages.enabled"] = false;
            GeckoPreferences.User["browser.download.manager.showAlertOnComplete"] = false;
            GeckoPreferences.User["browser.cache.disk.enable"] = false;
            GeckoPreferences.User["Browser.cache.check doc frequency"] = 3;

            GeckoPreferences.User["privacy.popups.showBrowserMessage"] = false;

            GeckoPreferences.User["security.warn_viewing_mixed"] = false;

            GeckoPreferences.User["javascript.options.mem.high_water_mark"] = 1536; // High memory usage at 1.5GB
            GeckoPreferences.User["javascript.options.mem.max"] = 512; // just below 2GB max ram usage (32 bit, so duh)
            GeckoPreferences.User["javascript.options.gc_on_memory_pressure"] = true;
            GeckoPreferences.User["javascript.options.mem.gc_allocation_threshold_mb"] = 256;
            GeckoPreferences.User["javascript.options.mem.gc_decommit_threshold_mb"] = 256;

            GeckoPreferences.User["javascript.options.mem.gc_high_frequency_high_limit_mb"] = 2000;
            GeckoPreferences.User["javascript.options.mem.gc_high_frequency_low_limit_mb"] = 256;

            GeckoPreferences.User["javascript.options.mem.gc_dynamic_heap_growth"] = true;
            GeckoPreferences.User["javascript.options.mem.gc_dynamic_mark_slice"] = true;
            GeckoPreferences.User["javascript.options.mem.gc_incremental"] = true;
            GeckoPreferences.User["javascript.options.mem.gc_incremental_slice_ms"] = 10000000;
            GeckoPreferences.User["javascript.options.mem.gc_per_compartment"] = true;
            GeckoPreferences.User["javascript.options.mem.gc_allocation_threshold_mb"] = 256;

            host = new WindowsFormsHost();
            browser = new GeckoWebBrowser();

            browser.NavigationError += NavigationError;
            browser.Load += BrowswerLoaded;
            browser.Disposed += BrowserDisposed;

            host.Child = browser;

            WebBrowserContainer.Children.Clear();
            WebBrowserContainer.Dispatcher.Invoke(() => WebBrowserContainer.Children.Add(host));
            if (currentUrl != null)
                Navigate(currentUrl);
        }

        private void BrowserDisposed(object sender, EventArgs e)
        {
            IsBrowserActive = false;
        }

        private void Navigate(string url)
        {
            currentUrl = url;
            if (browser != null)
                if (!browser.IsDisposed)
                {
                    if (Loading)    // The previous page didn't load
                    {
                        Beautifiers.LoadingNotificator.LoadingNotificator.LoadingFinished();
                        browser.Stop();
                    }

                    Beautifiers.LoadingNotificator.LoadingNotificator.LoadingInitialized();
                    Loading = true;

                    browser.Navigate(currentUrl, GeckoLoadFlags.AllowThirdPartyFixup);
                }
        }

        private void NavigationError(object sender, GeckoNavigationErrorEventArgs e)
        {
            //InitializeViewer();
        }

        private void BrowswerLoaded(object sender, DomEventArgs e)
        {
            Beautifiers.LoadingNotificator.LoadingNotificator.LoadingFinished();
            Loading = false;
        }

        private void AddRSSChannel_Button_Click(object sender, RoutedEventArgs e)
        {
            XmlReader xmlReader;
            try
            {
                xmlReader = XmlReader.Create(ChannelFinder);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("RSS not found");
                return;
            }
            Feeders.Add(new Feeder(ChannelFinder, xmlReader));

        }

        private void CloseBrowser_Button_Click(object sender, RoutedEventArgs e)
        {
            browser.Dispose();
        }

        private void MaximizeBrowser_Button_Click(object sender, RoutedEventArgs e)
        {
            IsBrowserNormal ^= true;
        }

        private void ShowBrowser_Button_Click(object sender, RoutedEventArgs e)
        {
            if (browser == null)
                InitializeViewer();
            IsBrowserNormal = true;
            Navigate(SelectedFeed.Link.ToString());
        }

        private void RefreshRSSChannels_Button_Click(object sender, RoutedEventArgs e)
        {
            ApiConn.GetSubscriptionsFromApiRest(Client.Id);
        }

    }
}
