using System;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LoginViewModel : ScreenViewModelBase
    {
        private readonly IAuthService _authService;
        private string _loginUserId;
        private string _password;
        private string _displayName;
        private string _authStatusText;
        private string _authStatusTone;
        private bool _isBusy;

        public LoginViewModel(UserSession session)
            : this(session, null)
        {
        }

        public LoginViewModel(UserSession session, IAuthService authService)
        {
            Session = session;
            _authService = authService;
            Title = "Login / Sign up";
            Description = "Only approved operators can enter the equipment control console.";
            LoginUserId = "operator01";
            DisplayName = "Operator";
            Department = "Process Equipment";
            AuthStatusText = authService == null ? "API NOT SET" : "API READY";
            AuthStatusTone = authService == null ? "Warning" : "Blue";
            LoginCommand = new RelayCommand(parameter => Login(), parameter => CanUseAuth());
            RegisterCommand = new RelayCommand(parameter => Register(), parameter => CanUseAuth());
            CheckStatusCommand = new RelayCommand(parameter => CheckStatus(), parameter => CanUseAuth() && !string.IsNullOrWhiteSpace(LoginUserId));
            LogoutCommand = new RelayCommand(parameter => Logout());
        }

        public UserSession Session { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand CheckStatusCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

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
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value)
                {
                    return;
                }

                _password = value;
                OnPropertyChanged();
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName == value)
                {
                    return;
                }

                _displayName = value;
                OnPropertyChanged();
            }
        }

        public string AuthStatusText
        {
            get { return _authStatusText; }
            private set
            {
                if (_authStatusText == value)
                {
                    return;
                }

                _authStatusText = value;
                OnPropertyChanged();
            }
        }

        public string AuthStatusTone
        {
            get { return _authStatusTone; }
            private set
            {
                if (_authStatusTone == value)
                {
                    return;
                }

                _authStatusTone = value;
                OnPropertyChanged();
            }
        }

        public string Department { get; set; }

        private bool CanUseAuth()
        {
            return !_isBusy && _authService != null;
        }

        private void Login()
        {
            RunAuthAction(() =>
            {
                var result = _authService.Login(LoginUserId, Password);
                Session.ApplyAuthResult(result);
                AuthStatusText = result != null && result.Success ? "LOGIN OK" : StatusText(result == null ? null : result.ApprovalStatus);
                AuthStatusTone = result != null && result.Success ? "Normal" : StatusTone(result == null ? null : result.ApprovalStatus);
                Description = result == null || string.IsNullOrWhiteSpace(result.Message) ? "Login response received from Flask API." : result.Message;
                OnPropertyChanged("Description");
            });
        }

        private void Register()
        {
            RunAuthAction(() =>
            {
                var result = _authService.Register(LoginUserId, Password, DisplayName);
                Session.ApplyRegisterResult(result);
                AuthStatusText = result != null && result.Success ? "REGISTERED" : "REGISTER FAIL";
                AuthStatusTone = result != null && result.Success ? "Warning" : "Danger";
                Description = result == null || string.IsNullOrWhiteSpace(result.Message) ? "Registration request was sent to Flask API." : result.Message;
                OnPropertyChanged("Description");
            });
        }

        private void CheckStatus()
        {
            RunAuthAction(() =>
            {
                var result = _authService.GetStatus(LoginUserId);
                if (result != null && string.IsNullOrWhiteSpace(result.UserId))
                {
                    result.UserId = LoginUserId;
                }

                if (result != null && string.IsNullOrWhiteSpace(result.ApprovalStatus))
                {
                    result.ApprovalStatus = result.Success ? "Approved" : "Pending";
                }

                if (result != null && string.Equals(result.ApprovalStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = true;
                }

                Session.ApplyAuthResult(result);
                AuthStatusText = StatusText(result == null ? null : result.ApprovalStatus);
                AuthStatusTone = StatusTone(result == null ? null : result.ApprovalStatus);
                Description = "Approval status was refreshed from Flask API.";
                OnPropertyChanged("Description");
            });
        }

        private void Logout()
        {
            Session.Logout();
            AuthStatusText = "SIGNED OUT";
            AuthStatusTone = "Disabled";
        }

        private void RunAuthAction(Action action)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                _isBusy = true;
                AuthStatusText = "REQUESTING";
                AuthStatusTone = "Blue";
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                action();
            }
            catch (Exception ex)
            {
                Session.Logout();
                AuthStatusText = "API ERROR";
                AuthStatusTone = "Danger";
                Description = ex.Message;
                OnPropertyChanged("Description");
            }
            finally
            {
                _isBusy = false;
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        private static string StatusText(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "UNKNOWN";
            }

            return status.Trim().ToUpperInvariant();
        }

        private static string StatusTone(string status)
        {
            if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                return "Normal";
            }

            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return "Warning";
            }

            return "Danger";
        }
    }
}
