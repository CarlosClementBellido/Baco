using System.Windows.Controls;

namespace Baco.Beautifiers.ToastNotificator
{
    /// <summary>
    /// Notificates the user with a bottom left message
    /// </summary>
    public static class ToastNotificator
    {

        /// <summary>
        /// Control to host the requested message
        /// </summary>
        public static ContentControl ToastNotificatorControl { get; set; }

        /// <summary>
        /// Makes appear a bottom left message
        /// </summary>
        /// <param name="dialogUserControl">Message</param>
        public static void Notificate(UserControl dialogUserControl)
        {
            ToastNotificatorControl.Content = dialogUserControl;
        }

    }
}
