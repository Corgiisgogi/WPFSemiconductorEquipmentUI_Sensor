namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LoginViewModel : ScreenViewModelBase
    {
        private string _loginUserId;

        public LoginViewModel(UserSession session)
        {
            Session = session;
            Title = "Login / Sign up";
            Description = "Only approved operators can enter the equipment control console.";
            LoginUserId = "operator01";
            Department = "Process Equipment";
            OperatorLoginCommand = new RelayCommand(LoginAsOperator);
            AdminLoginCommand = new RelayCommand(LoginAsAdmin);
            LogoutCommand = new RelayCommand(Logout);
        }

        public UserSession Session { get; private set; }
        public System.Windows.Input.ICommand OperatorLoginCommand { get; private set; }
        public System.Windows.Input.ICommand AdminLoginCommand { get; private set; }
        public System.Windows.Input.ICommand LogoutCommand { get; private set; }

        public string LoginUserId
        {
            get { return _loginUserId; }
            set
            {
                if (_loginUserId == value)
                {
                    return;
                }

                _loginUserId = value;
                OnPropertyChanged();
            }
        }

        public string Department { get; set; }

        private void LoginAsOperator(object parameter)
        {
            Session.LoginAsOperator(LoginUserId);
        }

        private void LoginAsAdmin(object parameter)
        {
            Session.LoginAsAdmin();
        }

        private void Logout(object parameter)
        {
            Session.Logout();
        }
    }
}
