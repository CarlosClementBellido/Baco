using System.Windows.Controls;
using System.Windows.Input;

namespace Baco.Windows.ProfileWindow
{
    /// <summary>
    /// Lógica de interacción para ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : UserControl
    {

        ProfileWindowVM profileWindowVM;

        public ProfileWindow()
        {
            InitializeComponent();
            profileWindowVM = new ProfileWindowVM();
            DataContext = profileWindowVM;
        }

        private void ChangeProfilePicture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            profileWindowVM.UpdateProfilePicture();
        }
    }
}
