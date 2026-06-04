using System.Windows.Controls;
using WPFSemiconductorEquipmentUI_Sensor.ViewModels;

namespace WPFSemiconductorEquipmentUI_Sensor.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as LoginViewModel;
            var passwordBox = sender as PasswordBox;
            if (viewModel != null && passwordBox != null)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}
