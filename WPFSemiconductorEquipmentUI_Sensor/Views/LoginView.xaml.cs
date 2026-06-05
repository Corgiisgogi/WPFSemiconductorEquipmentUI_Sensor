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

        private void OnLoginPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as LoginViewModel;
            var passwordBox = sender as PasswordBox;
            if (viewModel != null && passwordBox != null)
            {
                viewModel.LoginPassword = passwordBox.Password;
            }
        }

        private void OnRegisterPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as LoginViewModel;
            var passwordBox = sender as PasswordBox;
            if (viewModel != null && passwordBox != null)
            {
                viewModel.RegisterPassword = passwordBox.Password;
            }
        }

        private void OnRegisterConfirmPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as LoginViewModel;
            var passwordBox = sender as PasswordBox;
            if (viewModel != null && passwordBox != null)
            {
                viewModel.RegisterConfirmPassword = passwordBox.Password;
            }
        }
    }
}
