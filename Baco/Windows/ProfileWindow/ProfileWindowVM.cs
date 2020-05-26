using Baco.Api;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using static Baco.Utils.ImageUtils;

namespace Baco.Windows.ProfileWindow
{
    class ProfileWindowVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string Nickname { get; set; }
        public ImageSource ProfilePicture { get; set; } = null;

        public ProfileWindowVM()
        {
            ProfilePicture = ImageSourceFromBitmap(Resources.Resources._default);
            Nickname = Client.Nickname;

            Client.GetProfilePicture += GetProfilePicture;
            Client.ProfileImageUpdateFeedback += ProfileImageUpdateFeedback;

            ApiConn.GetProfilePictureFromApiRest(Client.Nickname);
        }

        private void ProfileImageUpdateFeedback(ApiResponse apiResponse)
        {
            if ((int)apiResponse.StatusCode >= 200 && (int)apiResponse.StatusCode < 300)
                ApiConn.GetProfilePictureFromApiRest(Client.Nickname);

        }

        private void GetProfilePicture(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                ProfilePicture.Dispatcher.Invoke(() => ProfilePicture = ImageSourceFromBitmap((Bitmap)Image.FromStream(ms)));
                Client.ProfilePicture = (Bitmap)image;
            }
        }

        internal void UpdateProfilePicture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                Title = "New Profile picture"
            };

            if (openFileDialog.ShowDialog().Value)
                ApiConn.ApiRestProfileUpdateRequestRequest(File.ReadAllBytes(openFileDialog.FileName));
        }
    }
}
