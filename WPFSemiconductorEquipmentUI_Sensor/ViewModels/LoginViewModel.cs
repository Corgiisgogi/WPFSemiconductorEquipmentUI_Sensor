using System;
using System.Windows;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LoginViewModel : ScreenViewModelBase
    {
        private readonly IAuthService _authService;
        private string _authMode;
        private string _loginUserId;
        private string _loginPassword;
        private string _registerUserId;
        private string _registerDisplayName;
        private string _registerPassword;
        private string _registerConfirmPassword;
        private string _authStatusText;
        private string _authStatusTone;
        private string _authMessage;
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
            RegisterUserId = "operator01";
            RegisterDisplayName = "Operator";
            Department = "Process Equipment";
            AuthMode = "Login";
            AuthStatusText = authService == null ? "API NOT SET" : "API READY";
            AuthStatusTone = authService == null ? "Warning" : "Blue";
            AuthMessage = "Login with an approved account, or switch to SIGN UP to request web admin approval.";
            ShowLoginCommand = new RelayCommand(parameter => ShowLogin());
            ShowSignUpCommand = new RelayCommand(parameter => ShowSignUp());
            LoginCommand = new RelayCommand(parameter => Login(), parameter => CanUseAuth());
            RegisterCommand = new RelayCommand(parameter => Register(), parameter => CanUseAuth());
            CheckStatusCommand = new RelayCommand(parameter => CheckStatus(), parameter => CanUseAuth() && !string.IsNullOrWhiteSpace(CurrentUserId));
            LogoutCommand = new RelayCommand(parameter => Logout());
        }

        public UserSession Session { get; private set; }
        public ICommand ShowLoginCommand { get; private set; }
        public ICommand ShowSignUpCommand { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand CheckStatusCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }

        public string AuthMode
        {
            get { return _authMode; }
            private set
            {
                if (_authMode == value)
                {
                    return;
                }

                _authMode = value;
                OnPropertyChanged();
                OnPropertyChanged("IsLoginMode");
                OnPropertyChanged("IsSignUpMode");
                OnPropertyChanged("LoginPanelVisibility");
                OnPropertyChanged("SignUpPanelVisibility");
                OnPropertyChanged("LoginModeTone");
                OnPropertyChanged("SignUpModeTone");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsLoginMode
        {
            get { return string.Equals(AuthMode, "Login", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsSignUpMode
        {
            get { return string.Equals(AuthMode, "SignUp", StringComparison.OrdinalIgnoreCase); }
        }

        public Visibility LoginPanelVisibility
        {
            get { return IsLoginMode ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility SignUpPanelVisibility
        {
            get { return IsSignUpMode ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string LoginModeTone
        {
            get { return IsLoginMode ? "Blue" : "Disabled"; }
        }

        public string SignUpModeTone
        {
            get { return IsSignUpMode ? "Blue" : "Disabled"; }
        }

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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string LoginPassword
        {
            get { return _loginPassword; }
            set
            {
                if (_loginPassword == value)
                {
                    return;
                }

                _loginPassword = value;
                OnPropertyChanged();
            }
        }

        public string RegisterUserId
        {
            get { return _registerUserId; }
            set
            {
                if (_registerUserId == value)
                {
                    return;
                }

                _registerUserId = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string RegisterDisplayName
        {
            get { return _registerDisplayName; }
            set
            {
                if (_registerDisplayName == value)
                {
                    return;
                }

                _registerDisplayName = value;
                OnPropertyChanged();
            }
        }

        public string RegisterPassword
        {
            get { return _registerPassword; }
            set
            {
                if (_registerPassword == value)
                {
                    return;
                }

                _registerPassword = value;
                OnPropertyChanged();
            }
        }

        public string RegisterConfirmPassword
        {
            get { return _registerConfirmPassword; }
            set
            {
                if (_registerConfirmPassword == value)
                {
                    return;
                }

                _registerConfirmPassword = value;
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

        public string AuthMessage
        {
            get { return _authMessage; }
            private set
            {
                if (_authMessage == value)
                {
                    return;
                }

                _authMessage = value;
                OnPropertyChanged();
            }
        }

        public string Department { get; set; }

        private string CurrentUserId
        {
            get { return IsSignUpMode ? RegisterUserId : LoginUserId; }
        }

        private bool CanUseAuth()
        {
            return !_isBusy && _authService != null;
        }

        private void ShowLogin()
        {
            AuthMode = "Login";
            AuthMessage = "Use an approved account to unlock equipment controls.";
            AuthStatusText = _authService == null ? "API NOT SET" : "LOGIN MODE";
            AuthStatusTone = _authService == null ? "Warning" : "Blue";
        }

        private void ShowSignUp()
        {
            AuthMode = "SignUp";
            if (string.IsNullOrWhiteSpace(RegisterUserId))
            {
                RegisterUserId = LoginUserId;
            }

            AuthMessage = "Create an account request. The account stays locked until the web admin approves it.";
            AuthStatusText = _authService == null ? "API NOT SET" : "SIGN UP MODE";
            AuthStatusTone = _authService == null ? "Warning" : "Blue";
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(LoginUserId) || string.IsNullOrWhiteSpace(LoginPassword))
            {
                SetInvalid("User ID and password are required for login.");
                return;
            }

            RunAuthAction(() =>
            {
                var result = _authService.Login(LoginUserId, LoginPassword);
                Session.ApplyAuthResult(result);
                AuthStatusText = result != null && result.Success ? "LOGIN OK" : StatusText(result == null ? null : result.ApprovalStatus);
                AuthStatusTone = result != null && result.Success ? "Normal" : StatusTone(result == null ? null : result.ApprovalStatus);
                AuthMessage = result == null || string.IsNullOrWhiteSpace(result.Message) ? "Login response received from Flask API." : result.Message;
            });
        }

        private void Register()
        {
            if (!ValidateRegisterInput())
            {
                return;
            }

            RunAuthAction(() =>
            {
                var result = _authService.Register(RegisterUserId, RegisterPassword, RegisterDisplayName);
                Session.ApplyRegisterResult(result);
                AuthStatusText = result != null && result.Success ? "PENDING" : "REGISTER FAIL";
                AuthStatusTone = result != null && result.Success ? "Warning" : "Danger";
                AuthMessage = result == null || string.IsNullOrWhiteSpace(result.Message)
                    ? "Registration submitted. Ask the web admin to approve this account, then press CHECK STATUS or LOGIN."
                    : result.Message;
                LoginUserId = RegisterUserId;
            });
        }

        private void CheckStatus()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                SetInvalid("User ID is required to check approval status.");
                return;
            }

            RunAuthAction(() =>
            {
                var result = _authService.GetStatus(userId);
                if (result != null && string.IsNullOrWhiteSpace(result.UserId))
                {
                    result.UserId = userId;
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
                AuthMessage = "Approval status was refreshed from Flask API.";
            });
        }

        private void Logout()
        {
            Session.Logout();
            AuthStatusText = "SIGNED OUT";
            AuthStatusTone = "Disabled";
            AuthMessage = "Session signed out. Login again with an approved account to control equipment.";
        }

        private bool ValidateRegisterInput()
        {
            if (string.IsNullOrWhiteSpace(RegisterUserId))
            {
                SetInvalid("User ID is required for sign up.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RegisterDisplayName))
            {
                SetInvalid("Display name is required for sign up.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RegisterPassword))
            {
                SetInvalid("Password is required for sign up.");
                return false;
            }

            if (!string.Equals(RegisterPassword, RegisterConfirmPassword, StringComparison.Ordinal))
            {
                SetInvalid("Password confirmation does not match.");
                return false;
            }

            return true;
        }

        private void SetInvalid(string message)
        {
            AuthStatusText = "INVALID";
            AuthStatusTone = "Danger";
            AuthMessage = message;
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
                AuthMessage = "Sending request to Flask API...";
                CommandManager.InvalidateRequerySuggested();
                action();
            }
            catch (Exception ex)
            {
                Session.Logout();
                AuthStatusText = "API ERROR";
                AuthStatusTone = "Danger";
                AuthMessage = ex.Message;
            }
            finally
            {
                _isBusy = false;
                CommandManager.InvalidateRequerySuggested();
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
