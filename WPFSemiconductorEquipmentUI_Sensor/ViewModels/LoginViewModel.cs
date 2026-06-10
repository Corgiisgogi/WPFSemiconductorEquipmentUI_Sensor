using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LoginViewModel : ScreenViewModelBase, IDisposable
    {
        private static readonly TimeSpan HealthCheckInterval = TimeSpan.FromSeconds(5);

        private readonly IAuthService _authService;
        private DispatcherTimer _healthTimer;
        private bool _healthCheckInFlight;
        private bool _disposed;
        private string _authMode;
        private string _loginUserId;
        private string _loginPassword;
        private string _registerUserId;
        private string _registerDisplayName;
        private string _registerPassword;
        private string _registerConfirmPassword;
        private string _apiStatusText;
        private string _apiStatusTone;
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
            ApiStatusText = authService == null ? "API 미설정" : "API 확인 전";
            ApiStatusTone = authService == null ? "Warning" : "Disabled";
            AuthStatusText = "대기";
            AuthStatusTone = "Disabled";
            AuthMessage = "승인된 계정으로 로그인하거나 회원가입을 요청하세요.";
            TestApiCommand = new RelayCommand(parameter => TestApi(), parameter => CanUseAuth());
            ShowLoginCommand = new RelayCommand(parameter => ShowLogin());
            ShowSignUpCommand = new RelayCommand(parameter => ShowSignUp());
            LoginCommand = new RelayCommand(parameter => Login(), parameter => CanUseAuth());
            RegisterCommand = new RelayCommand(parameter => Register(), parameter => CanUseAuth());
            LogoutCommand = new RelayCommand(parameter => Logout());
            Session.PropertyChanged += OnSessionPropertyChanged;
            if (_authService != null)
            {
                StartHealthMonitor();
            }
        }

        public UserSession Session { get; private set; }
        public ICommand TestApiCommand { get; private set; }
        public ICommand ShowLoginCommand { get; private set; }
        public ICommand ShowSignUpCommand { get; private set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
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

        // Flask API 연결 상태 전용(인증 결과와 분리). 주기 health 체크가 이 값만 갱신한다.
        public string ApiStatusText
        {
            get { return _apiStatusText; }
            private set
            {
                if (_apiStatusText == value)
                {
                    return;
                }

                _apiStatusText = value;
                OnPropertyChanged();
            }
        }

        public string ApiStatusTone
        {
            get { return _apiStatusTone; }
            private set
            {
                if (_apiStatusTone == value)
                {
                    return;
                }

                _apiStatusTone = value;
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

            if (e.PropertyName == "IsApproved")
            {
                // 로그인 중에는 배지가 안 보이고 갱신도 막히므로 불필요한 /api/health 폴링을 멈춘다.
                UpdateHealthMonitorState();
            }
        }

        private bool CanUseAuth()
        {
            return !_isBusy && _authService != null;
        }

        private void ShowLogin()
        {
            AuthMode = "Login";
            AuthMessage = "승인된 계정으로 로그인하면 장비 제어 권한이 활성화됩니다.";
        }

        private void ShowSignUp()
        {
            AuthMode = "SignUp";
            if (string.IsNullOrWhiteSpace(RegisterUserId))
            {
                RegisterUserId = LoginUserId;
            }

            AuthMessage = "회원가입 요청을 생성합니다. 웹 관리자가 승인할 때까지 계정은 잠금 상태입니다.";
        }

        private void StartHealthMonitor()
        {
            // 시작 직후 1회 검사 후, 주기적으로 재검사해 실시간으로 API 상태를 갱신한다.
            RefreshApiHealth();
            _healthTimer = new DispatcherTimer { Interval = HealthCheckInterval };
            _healthTimer.Tick += OnHealthTimerTick;
            _healthTimer.Start();
        }

        private void OnHealthTimerTick(object sender, EventArgs e)
        {
            RefreshApiHealth();
        }

        private void UpdateHealthMonitorState()
        {
            if (_healthTimer == null)
            {
                return;
            }

            // 로그인(승인) 상태에서는 폴링 중지, 로그아웃되면 재개.
            if (IsLoggedIn)
            {
                _healthTimer.Stop();
            }
            else if (!_healthTimer.IsEnabled)
            {
                _healthTimer.Start();
                RefreshApiHealth();
            }
        }

        // 주기 검사를 백그라운드 스레드에서 수행해 UI를 막지 않는다. 결과만 UI 스레드로 마샬링한다.
        private void RefreshApiHealth()
        {
            if (_authService == null || _disposed || IsLoggedIn || _healthCheckInFlight)
            {
                return;
            }

            _healthCheckInFlight = true;
            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                bool ok;
                try
                {
                    ok = _authService.CheckHealth();
                }
                catch
                {
                    ok = false;
                }

                Action apply = () =>
                {
                    _healthCheckInFlight = false;
                    if (_disposed || IsLoggedIn)
                    {
                        return;
                    }

                    ApplyHealthResult(ok);
                };

                if (dispatcher != null)
                {
                    dispatcher.Invoke(apply);
                }
                else
                {
                    apply();
                }
            });
        }

        // API 연결 배지만 갱신한다. 인증 결과(AuthStatusText/AuthMessage)는 건드리지 않아
        // 두 메시지가 겹치지 않는다.
        private void ApplyHealthResult(bool ok)
        {
            if (ok)
            {
                ApiStatusText = "API 정상";
                ApiStatusTone = "Normal";
            }
            else
            {
                ApiStatusText = "API 오류";
                ApiStatusTone = "Danger";
            }
        }

        // 수동 "API 확인" 명령: 주기 검사와 동일 경로로 즉시 재검사한다.
        private void TestApi()
        {
            RefreshApiHealth();
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
                    ? "회원가입 요청이 전송되었습니다. 웹 관리자 승인 후 로그인을 진행하세요."
                    : TranslateServerMessage(result.Message);
                LoginUserId = RegisterUserId;
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

                AuthStatusText = "요청 실패";
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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_healthTimer != null)
            {
                _healthTimer.Stop();
                _healthTimer.Tick -= OnHealthTimerTick;
                _healthTimer = null;
            }

            if (Session != null)
            {
                Session.PropertyChanged -= OnSessionPropertyChanged;
            }
        }
    }
}
