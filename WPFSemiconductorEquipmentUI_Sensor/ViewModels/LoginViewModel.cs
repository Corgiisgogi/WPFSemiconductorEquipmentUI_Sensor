using System;
using System.ComponentModel;
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
            Title = "로그인 / 회원가입";
            Description = "승인된 사용자만 장비 제어 콘솔을 사용할 수 있습니다.";
            LoginUserId = string.Empty;
            RegisterUserId = string.Empty;
            RegisterDisplayName = string.Empty;
            Department = "공정 장비";
            AuthMode = "Login";
            AuthStatusText = authService == null ? "API 미설정" : "API 확인 전";
            AuthStatusTone = authService == null ? "Warning" : "Disabled";
            AuthMessage = "Flask API 연결을 자동으로 확인하는 중입니다.";
            TestApiCommand = new RelayCommand(parameter => TestApi(), parameter => CanUseAuth());
            ShowLoginCommand = new RelayCommand(parameter => ShowLogin());
            ShowSignUpCommand = new RelayCommand(parameter => ShowSignUp());
            LoginCommand = new RelayCommand(parameter => Login(), parameter => CanUseAuth());
            RegisterCommand = new RelayCommand(parameter => Register(), parameter => CanUseAuth());
            CheckStatusCommand = new RelayCommand(parameter => CheckStatus(), parameter => CanUseAuth() && !string.IsNullOrWhiteSpace(CurrentUserId));
            LogoutCommand = new RelayCommand(parameter => Logout());
            Session.PropertyChanged += OnSessionPropertyChanged;
            if (_authService != null)
            {
                TestApi();
            }
        }

        public UserSession Session { get; private set; }
        public ICommand TestApiCommand { get; private set; }
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
                OnPropertyChanged("AuthEntryVisibility");
                OnPropertyChanged("LoggedInPanelVisibility");
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

        public bool IsLoggedIn
        {
            get { return Session != null && Session.IsApproved; }
        }

        public Visibility AuthEntryVisibility
        {
            get { return IsLoggedIn ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility LoggedInPanelVisibility
        {
            get { return IsLoggedIn ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility LoginPanelVisibility
        {
            get { return !IsLoggedIn && IsLoginMode ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility SignUpPanelVisibility
        {
            get { return !IsLoggedIn && IsSignUpMode ? Visibility.Visible : Visibility.Collapsed; }
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

        private void OnSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsApproved" || e.PropertyName == "UserId" || e.PropertyName == "RoleText" || e.PropertyName == "UserStateText")
            {
                OnPropertyChanged("IsLoggedIn");
                OnPropertyChanged("AuthEntryVisibility");
                OnPropertyChanged("LoggedInPanelVisibility");
                OnPropertyChanged("LoginPanelVisibility");
                OnPropertyChanged("SignUpPanelVisibility");
                CommandManager.InvalidateRequerySuggested();
            }
        }

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
            AuthMessage = "승인된 계정으로 로그인하면 장비 제어 권한이 활성화됩니다.";
            if (AuthStatusText != "API 정상" && AuthStatusText != "API 오류")
            {
                AuthStatusText = _authService == null ? "API 미설정" : "API 확인 전";
                AuthStatusTone = _authService == null ? "Warning" : "Disabled";
            }
        }

        private void ShowSignUp()
        {
            AuthMode = "SignUp";
            if (string.IsNullOrWhiteSpace(RegisterUserId))
            {
                RegisterUserId = LoginUserId;
            }

            AuthMessage = "회원가입 요청을 생성합니다. 웹 관리자가 승인할 때까지 계정은 잠금 상태입니다.";
            if (AuthStatusText != "API 정상" && AuthStatusText != "API 오류")
            {
                AuthStatusText = _authService == null ? "API 미설정" : "API 확인 전";
                AuthStatusTone = _authService == null ? "Warning" : "Disabled";
            }
        }

        private void TestApi()
        {
            RunAuthAction(() =>
            {
                if (_authService.CheckHealth())
                {
                    AuthStatusText = "API 정상";
                    AuthStatusTone = "Normal";
                    AuthMessage = "Flask API 연결 확인에 성공했습니다. 로그인/회원가입 요청을 보낼 수 있습니다.";
                }
                else
                {
                    AuthStatusText = "API 오류";
                    AuthStatusTone = "Danger";
                    AuthMessage = "Flask API 상태 확인 응답이 정상(200 OK)이 아닙니다.";
                }
            }, false);
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(LoginUserId) || string.IsNullOrWhiteSpace(LoginPassword))
            {
                SetInvalid("로그인에는 사용자 ID와 비밀번호가 필요합니다.");
                return;
            }

            RunAuthAction(() =>
            {
                var result = _authService.Login(LoginUserId, LoginPassword);
                Session.ApplyAuthResult(result);
                AuthStatusText = result != null && result.Success ? "로그인 완료" : StatusText(result == null ? null : result.ApprovalStatus);
                AuthStatusTone = result != null && result.Success ? "Normal" : StatusTone(result == null ? null : result.ApprovalStatus);
                AuthMessage = result == null || string.IsNullOrWhiteSpace(result.Message) ? "로그인 응답을 Flask API에서 수신했습니다." : TranslateServerMessage(result.Message);
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
                AuthStatusText = result != null && result.Success ? "승인 대기" : "가입 실패";
                AuthStatusTone = result != null && result.Success ? "Warning" : "Danger";
                AuthMessage = result == null || string.IsNullOrWhiteSpace(result.Message)
                    ? "회원가입 요청이 전송되었습니다. 웹 관리자 승인 후 상태 확인 또는 로그인을 진행하세요."
                    : TranslateServerMessage(result.Message);
                LoginUserId = RegisterUserId;
            });
        }

        private void CheckStatus()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                SetInvalid("승인 상태 확인에는 사용자 ID가 필요합니다.");
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
                AuthMessage = "Flask API에서 승인 상태를 갱신했습니다.";
            });
        }

        private void Logout()
        {
            Session.Logout();
            AuthStatusText = "로그아웃";
            AuthStatusTone = "Disabled";
            AuthMessage = "세션이 로그아웃되었습니다. 장비 제어를 위해 승인된 계정으로 다시 로그인하세요.";
        }

        private bool ValidateRegisterInput()
        {
            if (string.IsNullOrWhiteSpace(RegisterUserId))
            {
                SetInvalid("회원가입에는 사용자 ID가 필요합니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RegisterDisplayName))
            {
                SetInvalid("회원가입에는 표시 이름이 필요합니다.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(RegisterPassword))
            {
                SetInvalid("회원가입에는 비밀번호가 필요합니다.");
                return false;
            }

            if (!string.Equals(RegisterPassword, RegisterConfirmPassword, StringComparison.Ordinal))
            {
                SetInvalid("비밀번호 확인이 일치하지 않습니다.");
                return false;
            }

            return true;
        }

        private void SetInvalid(string message)
        {
            AuthStatusText = "입력 오류";
            AuthStatusTone = "Danger";
            AuthMessage = message;
        }

        private void RunAuthAction(Action action)
        {
            RunAuthAction(action, true);
        }

        private void RunAuthAction(Action action, bool logoutOnError)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                _isBusy = true;
                AuthStatusText = "요청 중";
                AuthStatusTone = "Blue";
                AuthMessage = "Flask API로 요청을 보내는 중입니다...";
                CommandManager.InvalidateRequerySuggested();
                action();
            }
            catch (Exception ex)
            {
                if (logoutOnError)
                {
                    Session.Logout();
                }

                AuthStatusText = "API 오류";
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
                return "알 수 없음";
            }

            return ToStatusDisplayText(status);
        }

        private static string ToStatusDisplayText(string status)
        {
            if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                return "승인됨";
            }

            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return "승인 대기";
            }

            if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return "거절됨";
            }

            if (string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
            {
                return "비활성";
            }

            return status.Trim();
        }

        private static string TranslateServerMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return message;
            }

            if (message.IndexOf("Login success", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "로그인 성공. 장비 제어 권한이 활성화되었습니다.";
            }

            if (message.IndexOf("Waiting for admin approval", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "회원가입 요청이 접수되었습니다. 웹 관리자 승인을 기다려 주세요.";
            }

            return message;
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
