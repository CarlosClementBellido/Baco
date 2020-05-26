using System.Collections.Generic;
using System.Linq;

namespace Baco.Debugging
{
    /// <summary>
    /// Shows the actions in a status bar
    /// </summary>
    public static partial class StatusBar
    {

        public static System.Windows.Controls.TextBlock MessageTextBlock { get; set; }
        private static List<VolatileString> StringStatus { get; set; }

        static StatusBar()
        {
            StringStatus = new List<VolatileString>();
        }

        public static void SetStatus(string message)
        {
            StringStatus.Add(new VolatileString(message));
            UpdateStatus();
        }

        public static void SetStatus(string message, int lifeTime)
        {
            StringStatus.Add(new VolatileString(message, lifeTime));
            UpdateStatus();
        }

        public static void UpdateStatus()
        {
            lock (MessageTextBlock)
            {
                lock (StringStatus)
                {
                    MessageTextBlock.Dispatcher.Invoke(() => MessageTextBlock.Text = string.Join(" - ", StringStatus.Select(s => s.Text)));
                }
            }
        }

    }
}
