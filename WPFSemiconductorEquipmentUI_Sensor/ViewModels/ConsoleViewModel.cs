using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class ConsoleViewModel : ScreenViewModelBase, IDisposable
    {
        private const int StaleReadThreshold = 6;
        private const int ProcessLampBit = 1; // NX_OD5121 DO2
        private const int WarningLampBit = 2; // NX_OD5121 DO3
        private const int AiControlLampBit = 3; // NX_OD5121 DO4
        private const double AiControlSafetyFactor = 0.95d; // AI 자동제어 ON 시 임계 초과값을 임계값의 95%로 보정
        private readonly ITrainerClient _trainerClient;
        private readonly ActivityLogStore _activityLogStore;
        private readonly SensorSnapshotRepository _sensorSnapshotRepository;
        private readonly IRemoteTelemetryService _remoteTelemetryService;
        private readonly AppSettingsStore _settings;
        private readonly DispatcherTimer _pollingTimer;
        private readonly UserSession _session;
        private bool _disposed;
        private bool _isReading;
        private int _successfulReadCount;
        private int _unchangedReadCount;
        private bool _hasLastRawSnapshot;
        private bool _hasDigitalInputCommandState;
        private bool _lastDigitalInput1;
        private bool _lastDigitalInput2;
        private bool _lastDigitalInput3;
        private bool _lastDigitalInput4;
        private short _lastPressureRaw;
        private short _lastVibrationRaw;
        private short _lastTemperatureRaw;
        private short _lastHumidityRaw;
        private DateTime _lastSensorSnapshotSavedAt;
        private DateTime _riskWindowStartedAt;
        private int _riskWarningCount;
        private bool _wasRiskWarningActive;
        private bool _autoShutdownIssued;
        private bool _forceShutdownAllowedByRisk;
        private bool _isProcessRunning;
        private bool _isAiControlRunning;
        private bool _isOpticalSensorOn;
        private bool _isInductiveSensorOn;
        private string _connectionStatusText;
        private string _connectionStatusTone;
        private string _lastUpdateText;
        private string _etherCatStatusText;
        private string _etherCatStatusTone;
        private string _summaryBadgeText;
        private string _summaryTone;
        private string _summaryText;
        private string _riskStatusText;
        private string _riskStatusTone;
        private string _riskDetailText;
        private string _digitalInput1Text;
        private string _digitalInput1Tone;
        private string _digitalInput2Text;
        private string _digitalInput2Tone;
        private string _digitalInput3Text;
        private string _digitalInput3Tone;
        private string _digitalInput4Text;
        private string _digitalInput4Tone;
        private string _opticalSensorText;
        private string _opticalSensorTone;
        private string _inductiveSensorText;
        private string _inductiveSensorTone;

        public ConsoleViewModel()
            : this(new UserSession(), new ActivityLogStore(), null, new AppSettingsStore(), null, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session)
            : this(session, new ActivityLogStore(), null, new AppSettingsStore(), null, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session, ActivityLogStore activityLogStore)
            : this(session, activityLogStore, null, new AppSettingsStore(), null, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(ITrainerClient trainerClient)
            : this(new UserSession(), new ActivityLogStore(), null, new AppSettingsStore(), null, trainerClient)
        {
        }

        public ConsoleViewModel(UserSession session, ITrainerClient trainerClient)
            : this(session, new ActivityLogStore(), null, new AppSettingsStore(), null, trainerClient)
        {
        }

        public ConsoleViewModel(UserSession session, ActivityLogStore activityLogStore, SensorSnapshotRepository sensorSnapshotRepository)
            : this(session, activityLogStore, sensorSnapshotRepository, new AppSettingsStore(), null, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session, ActivityLogStore activityLogStore, SensorSnapshotRepository sensorSnapshotRepository, AppSettingsStore settings)
            : this(session, activityLogStore, sensorSnapshotRepository, settings, null, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session, ActivityLogStore activityLogStore, SensorSnapshotRepository sensorSnapshotRepository, AppSettingsStore settings, IRemoteTelemetryService remoteTelemetryService)
            : this(session, activityLogStore, sensorSnapshotRepository, settings, remoteTelemetryService, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session, ActivityLogStore activityLogStore, SensorSnapshotRepository sensorSnapshotRepository, AppSettingsStore settings, IRemoteTelemetryService remoteTelemetryService, ITrainerClient trainerClient)
        {
            _session = session;
            _activityLogStore = activityLogStore;
            _sensorSnapshotRepository = sensorSnapshotRepository;
            _remoteTelemetryService = remoteTelemetryService;
            _settings = settings ?? new AppSettingsStore();
            _trainerClient = trainerClient;
            _riskWindowStartedAt = DateTime.MinValue;
            _session.PropertyChanged += OnSessionPropertyChanged;

            Title = "WPF 장비 제어 콘솔";
            Description = "센서 상태, 장비 제어, 작업 로그를 실시간으로 확인합니다.";

            Sensors = new ObservableCollection<SensorMetric>
            {
                new SensorMetric { Name = "압력", Unit = "bar", Status = SensorStatus.Idle },
                new SensorMetric { Name = "진동", Unit = "level", Status = SensorStatus.Idle },
                new SensorMetric { Name = "온도", Unit = "C", Status = SensorStatus.Idle },
                new SensorMetric { Name = "습도", Unit = "%", Status = SensorStatus.Idle }
            };

            ActivityLogs = _activityLogStore.Logs;

            ConnectionStatusText = "연결 안 됨";
            ConnectionStatusTone = "Disabled";
            LastUpdateText = "읽기 --:--:--";
            EtherCatStatusText = "연결 안 됨";
            EtherCatStatusTone = "Disabled";
            SummaryBadgeText = "대기";
            SummaryTone = "Disabled";
            SummaryText = "TwinCAT ADS 연결을 기다리는 중입니다. 읽기 실패 시 마지막 센서값은 화면에 유지됩니다.";
            RiskStatusText = "AI 준비";
            RiskStatusTone = "Normal";
            RiskDetailText = "압력, 진동, 온도, 습도 기준으로 위험 조건을 감시합니다.";
            SetDigitalInputs(false, false, false, false, false, false);
            TrySetStatusLamps(false, false, false);
            ProcessStartCommand = new RelayCommand(parameter => ExecuteControlCommand("DI1 Process Start", false), parameter => CanExecuteProcessStart());
            ProcessStopCommand = new RelayCommand(parameter => ExecuteControlCommand("DI2 Process Stop", false), parameter => CanExecuteProcessStop());
            AiControlStartCommand = new RelayCommand(parameter => ExecuteControlCommand("DI3 AI Control Start", true), parameter => CanExecuteAiControlStart());
            AiControlStopCommand = new RelayCommand(parameter => ExecuteControlCommand("DI4 AI Control Stop", true), parameter => CanExecuteAiControlStop());
            ForceShutdownCommand = new RelayCommand(parameter => ExecuteForceShutdown(false), parameter => CanExecuteForceShutdown());

            _pollingTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _pollingTimer.Tick += OnPollingTimerTick;
            _pollingTimer.Start();
        }

        public UserSession Session
        {
            get { return _session; }
        }

        public ObservableCollection<SensorMetric> Sensors { get; private set; }
        public ObservableCollection<ActivityLogItem> ActivityLogs { get; private set; }
        public ICommand ProcessStartCommand { get; private set; }
        public ICommand ProcessStopCommand { get; private set; }
        public ICommand AiControlStartCommand { get; private set; }
        public ICommand AiControlStopCommand { get; private set; }
        public ICommand ForceShutdownCommand { get; private set; }

        public string ProcessRunStateText
        {
            get { return _isProcessRunning ? "진행 중" : "정지"; }
        }

        public string ProcessRunStateTone
        {
            get { return _isProcessRunning ? "Normal" : "Disabled"; }
        }

        public string ProcessRunDetailText
        {
            get
            {
                if (_isProcessRunning)
                {
                    return "DI1 공정 시작 후 Running 상태입니다. DI2 공정 정지로 종료합니다.";
                }

                return IsFoupReady
                    ? "FOUP A/B 감지 완료. DI1 공정 시작으로 Running 상태로 전환합니다."
                    : "FOUP A/B 감지가 필요합니다. 광센서와 근접센서가 모두 ON일 때 공정 시작이 가능합니다.";
            }
        }

        public string FoupReadyText
        {
            get { return IsFoupReady ? "FOUP 준비" : "FOUP 대기"; }
        }

        public string FoupReadyTone
        {
            get { return IsFoupReady ? "Normal" : "Warning"; }
        }

        public string FoupReadyDetailText
        {
            get { return IsFoupReady ? "광센서와 근접센서가 모두 ON입니다." : "공정 시작 조건: 광센서 ON + 근접센서 ON"; }
        }

        private bool IsFoupReady
        {
            get { return _isOpticalSensorOn && _isInductiveSensorOn; }
        }

        public string AiRunStateText
        {
            get { return _isAiControlRunning ? "실행 중" : "정지"; }
        }

        public string AiRunStateTone
        {
            get { return _isAiControlRunning ? "Warning" : "Disabled"; }
        }

        public string AiRunDetailText
        {
            get { return _isAiControlRunning ? "AI 제어가 실행 중입니다. DI4 AI 제어 정지로 종료합니다." : "AI 제어 정지 상태입니다. 관리자 권한으로 DI3 AI 제어 시작을 실행할 수 있습니다."; }
        }

        public string AiControlAccessText
        {
            get { return _session.IsAdmin ? "관리자 가능" : "관리자 필요"; }
        }

        public string AiControlAccessTone
        {
            get { return _session.IsAdmin ? "Normal" : "Danger"; }
        }

        public string ForceShutdownAccessText
        {
            get { return _session.IsAdmin ? "가능" : (_forceShutdownAllowedByRisk ? "위험 허용" : "잠김"); }
        }

        public string ForceShutdownAccessTone
        {
            get { return _session.IsAdmin ? "Danger" : (_forceShutdownAllowedByRisk ? "Warning" : "Disabled"); }
        }

        public string ConnectionStatusText
        {
            get { return _connectionStatusText; }
            private set { SetProperty(ref _connectionStatusText, value); }
        }

        public string ConnectionStatusTone
        {
            get { return _connectionStatusTone; }
            private set { SetProperty(ref _connectionStatusTone, value); }
        }

        public string LastUpdateText
        {
            get { return _lastUpdateText; }
            private set { SetProperty(ref _lastUpdateText, value); }
        }

        public string EtherCatStatusText
        {
            get { return _etherCatStatusText; }
            private set { SetProperty(ref _etherCatStatusText, value); }
        }

        public string EtherCatStatusTone
        {
            get { return _etherCatStatusTone; }
            private set { SetProperty(ref _etherCatStatusTone, value); }
        }

        // 센서 표시(값/배지/색)는 View가 Sensors[i]에 직접 바인딩하고 컨버터로 변환한다.
        // 따라서 평면 *DisplayText/*StatusText/*StatusTone 속성과 Build* 헬퍼는 더 이상 두지 않는다.

        public string EquipmentStateText
        {
            get
            {
                if (string.Equals(RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase))
                {
                    return "DANGER";
                }

                if (string.Equals(RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase))
                {
                    return "WARNING";
                }

                if (_isProcessRunning)
                {
                    return "RUNNING";
                }

                return IsFoupReady ? "READY" : "IDLE";
            }
        }

        public string EquipmentStateTone
        {
            get
            {
                if (string.Equals(RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase))
                {
                    return "Danger";
                }

                if (string.Equals(RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase))
                {
                    return "Warning";
                }

                if (_isProcessRunning || IsFoupReady)
                {
                    return "Normal";
                }

                return "Disabled";
            }
        }

        public string FoupAStateText
        {
            get { return _isOpticalSensorOn ? "도킹" : "대기"; }
        }

        public string FoupAStateTone
        {
            get { return _isOpticalSensorOn ? "Normal" : "Disabled"; }
        }

        public string FoupBStateText
        {
            get { return _isInductiveSensorOn ? "도킹" : "대기"; }
        }

        public string FoupBStateTone
        {
            get { return _isInductiveSensorOn ? "Normal" : "Disabled"; }
        }

        public string Do2LampText
        {
            get { return _isProcessRunning ? "ON" : "OFF"; }
        }

        public string Do2LampTone
        {
            get { return _isProcessRunning ? "Normal" : "Disabled"; }
        }

        public string Do3LampText
        {
            get { return string.Equals(RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase) || string.Equals(RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF"; }
        }

        public string Do3LampTone
        {
            get
            {
                if (string.Equals(RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase))
                {
                    return "Danger";
                }

                return string.Equals(RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Disabled";
            }
        }

        public string Do4LampText
        {
            get { return _isAiControlRunning ? "ON" : "OFF"; }
        }

        public string Do4LampTone
        {
            get { return _isAiControlRunning ? "Blue" : "Disabled"; }
        }

        public string SummaryBadgeText
        {
            get { return _summaryBadgeText; }
            private set { SetProperty(ref _summaryBadgeText, value); }
        }

        public string SummaryTone
        {
            get { return _summaryTone; }
            private set { SetProperty(ref _summaryTone, value); }
        }

        public string SummaryText
        {
            get { return _summaryText; }
            private set { SetProperty(ref _summaryText, value); }
        }

        public string RiskStatusText
        {
            get { return _riskStatusText; }
            private set { SetProperty(ref _riskStatusText, value); }
        }

        public string RiskStatusTone
        {
            get { return _riskStatusTone; }
            private set { SetProperty(ref _riskStatusTone, value); }
        }

        public string RiskDetailText
        {
            get { return _riskDetailText; }
            private set { SetProperty(ref _riskDetailText, value); }
        }

        public string DigitalInput1Text
        {
            get { return _digitalInput1Text; }
            private set { SetProperty(ref _digitalInput1Text, value); }
        }

        public string DigitalInput1Tone
        {
            get { return _digitalInput1Tone; }
            private set { SetProperty(ref _digitalInput1Tone, value); }
        }

        public string DigitalInput2Text
        {
            get { return _digitalInput2Text; }
            private set { SetProperty(ref _digitalInput2Text, value); }
        }

        public string DigitalInput2Tone
        {
            get { return _digitalInput2Tone; }
            private set { SetProperty(ref _digitalInput2Tone, value); }
        }

        public string DigitalInput3Text
        {
            get { return _digitalInput3Text; }
            private set { SetProperty(ref _digitalInput3Text, value); }
        }

        public string DigitalInput3Tone
        {
            get { return _digitalInput3Tone; }
            private set { SetProperty(ref _digitalInput3Tone, value); }
        }

        public string DigitalInput4Text
        {
            get { return _digitalInput4Text; }
            private set { SetProperty(ref _digitalInput4Text, value); }
        }

        public string DigitalInput4Tone
        {
            get { return _digitalInput4Tone; }
            private set { SetProperty(ref _digitalInput4Tone, value); }
        }

        public string OpticalSensorText
        {
            get { return _opticalSensorText; }
            private set { SetProperty(ref _opticalSensorText, value); }
        }

        public string OpticalSensorTone
        {
            get { return _opticalSensorTone; }
            private set { SetProperty(ref _opticalSensorTone, value); }
        }

        public string InductiveSensorText
        {
            get { return _inductiveSensorText; }
            private set { SetProperty(ref _inductiveSensorText, value); }
        }

        public string InductiveSensorTone
        {
            get { return _inductiveSensorTone; }
            private set { SetProperty(ref _inductiveSensorTone, value); }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pollingTimer.Stop();
            _pollingTimer.Tick -= OnPollingTimerTick;
            _session.PropertyChanged -= OnSessionPropertyChanged;
            TrySetRunningLampOff();
            TrySetStatusLamps(false, false, false);
            _trainerClient.Dispose();
        }

        private void TrySetRunningLampOff()
        {
            try
            {
                _trainerClient.SetRunningLamp(false);
            }
            catch
            {
            }
        }

        private void OnSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsApproved" || e.PropertyName == "IsAdmin" || e.PropertyName == "RoleText" || e.PropertyName == "UserId")
            {
                OnPropertyChanged("AiControlAccessText");
                OnPropertyChanged("AiControlAccessTone");

                // 로그아웃(미승인 전이) 시 진행 중인 공정/AI 제어와 램프는 그대로 유지한다.
                // 권한 변화는 새 제어 명령의 허용 여부(CanExecute)에만 영향을 주고, 이미 동작 중인
                // 공정을 강제로 끄지는 않는다.

                OnPropertyChanged("ForceShutdownAccessText");
                OnPropertyChanged("ForceShutdownAccessTone");
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void TrySetStatusLamps(bool processOn, bool warningOn, bool aiOn)
        {
            TrySetStatusLamp(ProcessLampBit, processOn);
            TrySetStatusLamp(WarningLampBit, warningOn);
            TrySetStatusLamp(AiControlLampBit, aiOn);
        }

        private void TrySetStatusLamp(int bitIndex, bool isOn)
        {
            try
            {
                _trainerClient.SetDigitalOutput(bitIndex, isOn);
            }
            catch
            {
            }
        }

        private void SyncProcessLamp()
        {
            TrySetStatusLamp(ProcessLampBit, _isProcessRunning);
        }

        private void SyncAiControlLamp()
        {
            TrySetStatusLamp(AiControlLampBit, _isAiControlRunning);
        }

        private void SyncWarningLamp(bool isOn)
        {
            TrySetStatusLamp(WarningLampBit, isOn);
        }

        // 센서 셀(값/배지/색)은 Sensors[i]의 INotifyPropertyChanged로 자동 갱신되므로
        // 여기서는 센서 상태에 연동되는 대시보드(장비상태/FOUP/램프)만 알린다.
        private void NotifySensorDisplayChanged()
        {
            NotifyDashboardStateChanged();
        }

        private void NotifyDashboardStateChanged()
        {
            OnPropertyChanged("EquipmentStateText");
            OnPropertyChanged("EquipmentStateTone");
            OnPropertyChanged("FoupAStateText");
            OnPropertyChanged("FoupAStateTone");
            OnPropertyChanged("FoupBStateText");
            OnPropertyChanged("FoupBStateTone");
            OnPropertyChanged("Do2LampText");
            OnPropertyChanged("Do2LampTone");
            OnPropertyChanged("Do3LampText");
            OnPropertyChanged("Do3LampTone");
            OnPropertyChanged("Do4LampText");
            OnPropertyChanged("Do4LampTone");
        }

        private bool CanExecuteApprovedCommand()
        {
            return !_disposed && _session.IsApproved;
        }

        private bool CanExecuteProcessStart()
        {
            return CanExecuteApprovedCommand() && !_isProcessRunning && IsFoupReady;
        }

        private bool CanExecuteProcessStop()
        {
            return CanExecuteApprovedCommand() && _isProcessRunning;
        }

        private bool CanExecuteAdminCommand()
        {
            return !_disposed && _session.IsAdmin;
        }

        private bool CanExecuteAiControlStart()
        {
            return CanExecuteAdminCommand() && !_isAiControlRunning && _isProcessRunning;
        }

        private bool CanExecuteAiControlStop()
        {
            return CanExecuteAdminCommand() && _isAiControlRunning;
        }

        private bool CanExecuteForceShutdown()
        {
            return !_disposed && (_session.IsAdmin || _forceShutdownAllowedByRisk);
        }

        private void ExecuteControlCommand(string commandName, bool adminOnly)
        {
            if (adminOnly && !_session.IsAdmin)
            {
                AddActivityLog("제어", ToDisplayCommandName(commandName) + " 차단 - 관리자 권한 필요", "RISK");
                return;
            }

            if (!_session.IsApproved)
            {
                AddActivityLog("제어", ToDisplayCommandName(commandName) + " 차단 - 승인된 로그인 필요", "RISK");
                return;
            }

            if (!ApplyControlStateTransition(commandName))
            {
                return;
            }

            AddActivityLog("제어", ToDisplayCommandName(commandName) + " 명령 승인", adminOnly ? "WARN" : "INFO");
            SummaryBadgeText = ToDisplayCommandName(commandName);
            SummaryTone = adminOnly ? "Warning" : "Normal";
            SummaryText = BuildControlSummaryText(commandName);
        }

        private void ExecuteForceShutdown(bool automatic)
        {
            // AI 위험 조건이 활성(_forceShutdownAllowedByRisk)이면 admin/승인 로그인 여부와 무관하게
            // 비로그인 상태에서도 수동 강제 정지를 허용한다. 위험이 아닐 때만 기존 권한(admin+승인)을 요구한다.
            if (!automatic && !_forceShutdownAllowedByRisk)
            {
                if (!_session.IsAdmin)
                {
                    AddActivityLog("제어", "강제 정지 차단 - 관리자 또는 AI 위험 조건 필요", "RISK");
                    return;
                }

                if (!_session.IsApproved)
                {
                    AddActivityLog("제어", "강제 정지 차단 - 승인된 로그인 필요", "RISK");
                    return;
                }
            }

            _isProcessRunning = false;
            _isAiControlRunning = false;
            NotifyProcessStateChanged();
            NotifyAiStateChanged();
            SyncWarningLamp(true);
            NotifyDashboardStateChanged();
            AddActivityLog(automatic ? "AI 규칙" : "제어", automatic ? "자동 강제 정지 이벤트 발생" : "강제 정지 이벤트 발생", "RISK");
            SummaryBadgeText = automatic ? "자동 정지" : "강제 정지";
            SummaryTone = "Danger";
            SummaryText = "WPF 제어 계층에서 강제 정지 이벤트가 발생했습니다. NX_OD5121 DO2~DO4는 상태 램프 전용이므로 이 버튼에서 pulse 출력하지 않습니다.";
        }

        private bool ApplyControlStateTransition(string commandName)
        {
            if (string.Equals(commandName, "DI1 Process Start", StringComparison.OrdinalIgnoreCase))
            {
                if (_isProcessRunning)
                {
                    AddActivityLog("제어", "DI1 공정 시작 차단 - 이미 진행 중", "WARN");
                    return false;
                }

                if (!IsFoupReady)
                {
                    AddActivityLog("제어", "DI1 공정 시작 차단 - FOUP A/B 감지 필요", "WARN");
                    SummaryBadgeText = "공정 대기";
                    SummaryTone = "Warning";
                    SummaryText = "광센서와 근접센서가 모두 ON일 때 공정 시작이 가능합니다.";
                    return false;
                }

                _isProcessRunning = true;
                NotifyProcessStateChanged();
                return true;
            }

            if (string.Equals(commandName, "DI2 Process Stop", StringComparison.OrdinalIgnoreCase))
            {
                if (!_isProcessRunning)
                {
                    AddActivityLog("제어", "DI2 공정 정지 차단 - 공정이 이미 정지 상태", "WARN");
                    return false;
                }

                _isProcessRunning = false;
                NotifyProcessStateChanged();

                // 공정이 정지하면 진행 중인 AI 제어도 함께 정지한다. AI 자동제어는 공정이 돌아가는 동안에만 의미가 있다.
                if (_isAiControlRunning)
                {
                    _isAiControlRunning = false;
                    NotifyAiStateChanged();
                    AddActivityLog("제어", "공정 정지에 따라 AI 제어도 정지", "WARN");
                }

                return true;
            }

            if (string.Equals(commandName, "DI3 AI Control Start", StringComparison.OrdinalIgnoreCase))
            {
                if (_isAiControlRunning)
                {
                    AddActivityLog("제어", "DI3 AI 제어 시작 차단 - 이미 실행 중", "WARN");
                    return false;
                }

                if (!_isProcessRunning)
                {
                    AddActivityLog("제어", "DI3 AI 제어 시작 차단 - 공정 진행 필요", "WARN");
                    return false;
                }

                _isAiControlRunning = true;
                NotifyAiStateChanged();
                return true;
            }

            if (string.Equals(commandName, "DI4 AI Control Stop", StringComparison.OrdinalIgnoreCase))
            {
                if (!_isAiControlRunning)
                {
                    AddActivityLog("제어", "DI4 AI 제어 정지 차단 - AI 제어가 이미 정지 상태", "WARN");
                    return false;
                }

                _isAiControlRunning = false;
                NotifyAiStateChanged();
                return true;
            }

            return true;
        }

        private string BuildControlSummaryText(string commandName)
        {
            if (string.Equals(commandName, "DI1 Process Start", StringComparison.OrdinalIgnoreCase))
            {
                return "FOUP A/B 감지 조건을 만족하여 공정 상태가 Running으로 전환되었습니다.";
            }

            if (string.Equals(commandName, "DI2 Process Stop", StringComparison.OrdinalIgnoreCase))
            {
                return "공정 상태가 정지로 전환되었습니다. DI1 공정 시작으로 다시 Running 상태가 됩니다.";
            }

            if (string.Equals(commandName, "DI3 AI Control Start", StringComparison.OrdinalIgnoreCase))
            {
                return "AI 제어 상태가 실행 중으로 전환되었습니다. DI4 AI 제어 정지 명령 전까지 실행 중으로 표시됩니다.";
            }

            if (string.Equals(commandName, "DI4 AI Control Stop", StringComparison.OrdinalIgnoreCase))
            {
                return "AI 제어 상태가 정지로 전환되었습니다.";
            }

            return ToDisplayCommandName(commandName) + " 명령이 승인되었습니다.";
        }

        private void NotifyProcessStateChanged()
        {
            SyncProcessLamp();
            OnPropertyChanged("ProcessRunStateText");
            OnPropertyChanged("ProcessRunStateTone");
            OnPropertyChanged("ProcessRunDetailText");
            NotifyDashboardStateChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        private void NotifyAiStateChanged()
        {
            SyncAiControlLamp();
            OnPropertyChanged("AiRunStateText");
            OnPropertyChanged("AiRunStateTone");
            OnPropertyChanged("AiRunDetailText");
            NotifyDashboardStateChanged();
            CommandManager.InvalidateRequerySuggested();
        }

        private static string ToDisplayCommandName(string commandName)
        {
            if (string.Equals(commandName, "DI1 Process Start", StringComparison.OrdinalIgnoreCase))
            {
                return "DI1 공정 시작";
            }

            if (string.Equals(commandName, "DI2 Process Stop", StringComparison.OrdinalIgnoreCase))
            {
                return "DI2 공정 정지";
            }

            if (string.Equals(commandName, "DI3 AI Control Start", StringComparison.OrdinalIgnoreCase))
            {
                return "DI3 AI 제어 시작";
            }

            if (string.Equals(commandName, "DI4 AI Control Stop", StringComparison.OrdinalIgnoreCase))
            {
                return "DI4 AI 제어 정지";
            }

            return commandName;
        }

        private void AddActivityLog(string source, string eventText, string severity)
        {
            _activityLogStore.Add(source, _session.UserId, eventText, severity);
        }

        private void OnPollingTimerTick(object sender, EventArgs e)
        {
            if (_disposed || _isReading)
            {
                return;
            }

            _isReading = true;
            try
            {
                var snapshot = _trainerClient.ReadSnapshot();
                if (IsStaleSnapshot(snapshot))
                {
                    TrySetRunningLampOff();
                    ApplyStaleSnapshot(snapshot);
                }
                else
                {
                    _trainerClient.SetRunningLamp(true);
                    ApplySnapshot(snapshot);
                }
            }
            catch (Exception ex)
            {
                TrySetRunningLampOff();
                ApplyReadFailure(ex);
            }
            finally
            {
                _isReading = false;
            }
        }

        private bool IsStaleSnapshot(SensorTrainerSnapshot snapshot)
        {
            if (!_hasLastRawSnapshot)
            {
                SaveLastRawSnapshot(snapshot);
                return false;
            }

            if (_lastPressureRaw == snapshot.Pressure
                && _lastVibrationRaw == snapshot.Vibration
                && _lastTemperatureRaw == snapshot.Temperature
                && _lastHumidityRaw == snapshot.Humidity)
            {
                _unchangedReadCount++;
            }
            else
            {
                _unchangedReadCount = 0;
                SaveLastRawSnapshot(snapshot);
            }

            return _unchangedReadCount >= StaleReadThreshold;
        }

        private void SaveLastRawSnapshot(SensorTrainerSnapshot snapshot)
        {
            _lastPressureRaw = snapshot.Pressure;
            _lastVibrationRaw = snapshot.Vibration;
            _lastTemperatureRaw = snapshot.Temperature;
            _lastHumidityRaw = snapshot.Humidity;
            _hasLastRawSnapshot = true;
        }

        private void ApplySnapshot(SensorTrainerSnapshot snapshot)
        {
            if (_disposed)
            {
                return;
            }

            _successfulReadCount++;

            var pressure = UpdateSensor(Sensors[0], snapshot.Pressure, CalibratePressure);
            var vibration = UpdateSensor(Sensors[1], snapshot.Vibration, CalibrateVibration);
            var temperature = UpdateSensor(Sensors[2], snapshot.Temperature, CalibrateTemperature);
            var humidity = UpdateSensor(Sensors[3], snapshot.Humidity, CalibrateHumidity);
            if (_isAiControlRunning)
            {
                pressure = ApplyAiControlCorrection(Sensors[0], pressure, _settings.PressureWarningThreshold, 0d, 0.45d);
                vibration = ApplyAiControlCorrection(Sensors[1], vibration, _settings.VibrationWarningThreshold, 0d, 10d);
                temperature = ApplyAiControlCorrection(Sensors[2], temperature, _settings.TemperatureWarningThreshold, 0d, 60d);
                humidity = ApplyAiControlCorrection(Sensors[3], humidity, _settings.HumidityWarningThreshold, 0d, 100d);
            }

            NotifySensorDisplayChanged();

            SetDigitalInputs(
                snapshot.DigitalInput1,
                snapshot.DigitalInput2,
                snapshot.DigitalInput3,
                snapshot.DigitalInput4,
                snapshot.OpticalSensor,
                snapshot.InductiveSensor);
            HandleDigitalInputCommands(snapshot);
            EvaluateRiskRules(pressure, vibration, temperature, humidity);
            TrySaveSensorSnapshot(snapshot, pressure, vibration, temperature, humidity);

            ConnectionStatusText = "ADS 정상";
            ConnectionStatusTone = "Normal";
            LastUpdateText = "읽기 #" + _successfulReadCount + " " + snapshot.ReceivedAt.ToString("HH:mm:ss");
            EtherCatStatusText = "준비";
            EtherCatStatusTone = "Normal";

            if (SummaryTone != "Danger")
            {
                SummaryBadgeText = "PLC 연결";
                SummaryTone = "Normal";
                SummaryText = "TwinCAT ADS 읽기 정상. 센서값을 AI 위험 규칙으로 감시 중입니다.";
            }
        }

        private void ApplyStaleSnapshot(SensorTrainerSnapshot snapshot)
        {
            if (_disposed)
            {
                return;
            }

            _successfulReadCount++;

            var pressure = UpdateSensor(Sensors[0], snapshot.Pressure, CalibratePressure);
            var vibration = UpdateSensor(Sensors[1], snapshot.Vibration, CalibrateVibration);
            var temperature = UpdateSensor(Sensors[2], snapshot.Temperature, CalibrateTemperature);
            var humidity = UpdateSensor(Sensors[3], snapshot.Humidity, CalibrateHumidity);
            if (_isAiControlRunning)
            {
                pressure = ApplyAiControlCorrection(Sensors[0], pressure, _settings.PressureWarningThreshold, 0d, 0.45d);
                vibration = ApplyAiControlCorrection(Sensors[1], vibration, _settings.VibrationWarningThreshold, 0d, 10d);
                temperature = ApplyAiControlCorrection(Sensors[2], temperature, _settings.TemperatureWarningThreshold, 0d, 60d);
                humidity = ApplyAiControlCorrection(Sensors[3], humidity, _settings.HumidityWarningThreshold, 0d, 100d);
            }

            SetSensorStale(Sensors[0]);
            SetSensorStale(Sensors[1]);
            SetSensorStale(Sensors[2]);
            SetSensorStale(Sensors[3]);
            NotifySensorDisplayChanged();

            SetDigitalInputs(
                snapshot.DigitalInput1,
                snapshot.DigitalInput2,
                snapshot.DigitalInput3,
                snapshot.DigitalInput4,
                snapshot.OpticalSensor,
                snapshot.InductiveSensor);
            HandleDigitalInputCommands(snapshot);
            EvaluateRiskRules(pressure, vibration, temperature, humidity);
            TrySaveSensorSnapshot(snapshot, pressure, vibration, temperature, humidity);

            ConnectionStatusText = "값 정지";
            ConnectionStatusTone = "Warning";
            LastUpdateText = "읽기 #" + _successfulReadCount + " " + snapshot.ReceivedAt.ToString("HH:mm:ss");
            EtherCatStatusText = "값 정지";
            EtherCatStatusTone = "Warning";

            if (SummaryTone != "Danger")
            {
                SummaryBadgeText = "값 정지";
                SummaryTone = "Warning";
                SummaryText = "PLC 원시 센서값이 약 3초 동안 변하지 않았습니다. ADS는 읽히지만 센서 입력 갱신이 멈춘 것으로 보입니다.";
            }
        }


        private void TrySaveSensorSnapshot(
            SensorTrainerSnapshot snapshot,
            double pressure,
            double vibration,
            double temperature,
            double humidity)
        {
            if (_sensorSnapshotRepository == null)
            {
                return;
            }

            var now = DateTime.Now;
            if (_lastSensorSnapshotSavedAt != DateTime.MinValue
                && (now - _lastSensorSnapshotSavedAt).TotalSeconds < _settings.SensorSnapshotSaveIntervalSeconds)
            {
                return;
            }

            try
            {
                var record = new SensorSnapshotRecord
                {
                    CapturedAt = snapshot.ReceivedAt,
                    PressureRaw = snapshot.Pressure,
                    PressureValue = pressure,
                    VibrationRaw = snapshot.Vibration,
                    VibrationValue = vibration,
                    TemperatureRaw = snapshot.Temperature,
                    TemperatureValue = temperature,
                    HumidityRaw = snapshot.Humidity,
                    HumidityValue = humidity,
                    DigitalInput1 = snapshot.DigitalInput1,
                    DigitalInput2 = snapshot.DigitalInput2,
                    DigitalInput3 = snapshot.DigitalInput3,
                    DigitalInput4 = snapshot.DigitalInput4,
                    OpticalSensor = snapshot.OpticalSensor,
                    InductiveSensor = snapshot.InductiveSensor
                };
                _sensorSnapshotRepository.Insert(record);
                if (_remoteTelemetryService != null)
                {
                    _remoteTelemetryService.SendSensorSnapshot(record);
                }

                _lastSensorSnapshotSavedAt = now;
            }
            catch (Exception ex)
            {
                AddActivityLog("저장소", "센서 스냅샷 저장 실패: " + ex.Message, "WARN");
                _lastSensorSnapshotSavedAt = now;
            }
        }

        private void ApplyReadFailure(Exception ex)
        {
            if (_disposed)
            {
                return;
            }

            _forceShutdownAllowedByRisk = false;
            NotifyForceShutdownStateChanged();
            ConnectionStatusText = "읽기 오류";
            ConnectionStatusTone = "Danger";
            EtherCatStatusText = "연결 안 됨";
            EtherCatStatusTone = "Danger";
            RiskStatusText = "AI 일시정지";
            RiskStatusTone = "Disabled";
            SyncWarningLamp(false);
            NotifyDashboardStateChanged();
            RiskDetailText = "ADS 읽기 실패. 다음 정상 센서 스냅샷까지 위험 규칙을 대기합니다.";
            SummaryBadgeText = "읽기 오류";
            SummaryTone = "Danger";
            SummaryText = "TwinCAT ADS 데이터를 읽을 수 없습니다. TwinCAT 런타임, ADS Route, 851 포트, GVL.NX_* 변수를 확인하세요. " + ex.Message;
        }

        private void EvaluateRiskRules(double pressure, double vibration, double temperature, double humidity)
        {
            var warningCount = 0;
            var details = string.Empty;

            if (pressure >= _settings.PressureWarningThreshold)
            {
                warningCount++;
                details = AppendRiskDetail(details, "압력 " + pressure.ToString("0.00") + " bar");
                SetSensorWarning(Sensors[0]);
            }

            if (vibration >= _settings.VibrationWarningThreshold)
            {
                warningCount++;
                details = AppendRiskDetail(details, "진동 " + vibration.ToString("0.0"));
                SetSensorWarning(Sensors[1]);
            }

            if (temperature >= _settings.TemperatureWarningThreshold)
            {
                warningCount++;
                details = AppendRiskDetail(details, "온도 " + temperature.ToString("0.0") + " C");
                SetSensorWarning(Sensors[2]);
            }

            if (humidity >= _settings.HumidityWarningThreshold)
            {
                warningCount++;
                details = AppendRiskDetail(details, "습도 " + humidity.ToString("0.0") + " %");
                SetSensorWarning(Sensors[3]);
            }

            var now = DateTime.Now;

            // 윈도우 만료 검사는 정상/경고 두 경로 모두에서 선행 수행한다. 만료 시에만 누적 카운트와
            // 자동 종료 발동 플래그를 리셋해, 윈도우가 살아있는 동안에는 별개 경고 이벤트가 누적되도록 한다.
            if (_riskWindowStartedAt != DateTime.MinValue && (now - _riskWindowStartedAt).TotalSeconds > _settings.RiskWindowSeconds)
            {
                _riskWarningCount = 0;
                _riskWindowStartedAt = DateTime.MinValue;
                _autoShutdownIssued = false;
            }

            if (warningCount == 0)
            {
                // 현재 위험 표시만 해제하고 카운트/윈도우는 유지한다. 윈도우 내에 다시 경고가 발생하면
                // 별개 이벤트로 누적되어야 자동 종료(예: 60초 내 2회)가 가능하다.
                _wasRiskWarningActive = false;
                _forceShutdownAllowedByRisk = false;
                RiskStatusText = "AI 정상";
                RiskStatusTone = "Normal";
                SyncWarningLamp(false);
                NotifyDashboardStateChanged();
                if (_riskWarningCount > 0 && _riskWindowStartedAt != DateTime.MinValue)
                {
                    RiskDetailText = "위험 기준 초과 없음. " + _settings.RiskWindowSeconds + "초 내 위험 카운트 " + _riskWarningCount + "/" + _settings.AutoShutdownWarningLimit + " 유지 중.";
                }
                else
                {
                    RiskDetailText = "위험 기준 초과 없음.";
                }

                NotifyForceShutdownStateChanged();
                return;
            }

            if (_riskWindowStartedAt == DateTime.MinValue)
            {
                _riskWindowStartedAt = now;
            }

            // 경고 "진입"(정상→경고 전이)일 때만 1회로 센다. 동시에 여러 센서가 초과해도 진입 1회이며,
            // 경고가 여러 폴링에 걸쳐 지속돼도 중복 카운트하지 않는다.
            if (!_wasRiskWarningActive)
            {
                _riskWarningCount += 1;
                AddActivityLog("AI 규칙", details + " 기준 초과. " + _settings.RiskWindowSeconds + "초 내 위험 카운트 " + _riskWarningCount + "/" + _settings.AutoShutdownWarningLimit, _riskWarningCount >= _settings.AutoShutdownWarningLimit ? "RISK" : "WARN");
            }

            _wasRiskWarningActive = true;
            _forceShutdownAllowedByRisk = true;
            RiskStatusText = _riskWarningCount >= _settings.AutoShutdownWarningLimit ? "AI 위험" : "AI 경고";
            RiskStatusTone = _riskWarningCount >= _settings.AutoShutdownWarningLimit ? "Danger" : "Warning";
            SyncWarningLamp(true);
            NotifyDashboardStateChanged();
            RiskDetailText = details + " 기준 초과. " + _settings.RiskWindowSeconds + "초 내 위험 카운트 " + _riskWarningCount + "/" + _settings.AutoShutdownWarningLimit;
            NotifyForceShutdownStateChanged();

            if (_riskWarningCount >= _settings.AutoShutdownWarningLimit && !_autoShutdownIssued)
            {
                _autoShutdownIssued = true;
                ExecuteForceShutdown(true);
            }
        }

        private static string AppendRiskDetail(string current, string next)
        {
            return string.IsNullOrEmpty(current) ? next : current + ", " + next;
        }

        private void NotifyForceShutdownStateChanged()
        {
            OnPropertyChanged("ForceShutdownAccessText");
            OnPropertyChanged("ForceShutdownAccessTone");
            CommandManager.InvalidateRequerySuggested();
        }

        private void SetSensorStale(SensorMetric sensor)
        {
            sensor.Status = SensorStatus.Stale;
        }

        private void SetSensorWarning(SensorMetric sensor)
        {
            sensor.Status = SensorStatus.Warning;
        }

        private double UpdateSensor(
            SensorMetric sensor,
            short rawValue,
            Func<short, double> calibration)
        {
            var calibratedValue = calibration(rawValue);

            sensor.RawValue = rawValue;
            sensor.NumericValue = calibratedValue;
            sensor.Status = SensorStatus.Normal;
            return calibratedValue;
        }

        // AI 자동제어 ON 시 임계값을 초과한 센서값을 임계값 바로 아래로 끌어내려 경고가 발생하지 않게 한다.
        // 보정값은 화면 표시와 위험 평가 모두에 사용된다(원시 RawValue·스냅샷에는 원래 값이 그대로 유지).
        private double ApplyAiControlCorrection(
            SensorMetric sensor,
            double value,
            double warningThreshold,
            double minimum,
            double maximum)
        {
            if (value < warningThreshold)
            {
                return value;
            }

            var corrected = Clamp(warningThreshold * AiControlSafetyFactor, minimum, maximum);
            sensor.NumericValue = corrected;
            sensor.Status = SensorStatus.AiCorrected;
            return corrected;
        }

        private static double CalibratePressure(short rawValue)
        {
            return (rawValue * 0.00016129032258064516d) - 0.1532258064516129d;
        }

        private static double CalibrateVibration(short rawValue)
        {
            return Clamp((rawValue * 0.001282051282051282d) + 0.0512820512820511d, 0d, 10d);
        }

        private static double CalibrateTemperature(short rawValue)
        {
            return (rawValue * 0.014035087719298246d) - 39.55789473684211d;
        }

        private static double CalibrateHumidity(short rawValue)
        {
            return (rawValue * 0.010344827586206896d) + 5.793103448275861d;
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }

        private void SetDigitalInputs(
            bool digitalInput1,
            bool digitalInput2,
            bool digitalInput3,
            bool digitalInput4,
            bool opticalSensor,
            bool inductiveSensor)
        {
            var foupReadyBefore = IsFoupReady;
            _isOpticalSensorOn = opticalSensor;
            _isInductiveSensorOn = inductiveSensor;
            if (foupReadyBefore != IsFoupReady)
            {
                OnPropertyChanged("FoupReadyText");
                OnPropertyChanged("FoupReadyTone");
                OnPropertyChanged("FoupReadyDetailText");
                OnPropertyChanged("ProcessRunDetailText");
                NotifyDashboardStateChanged();
                CommandManager.InvalidateRequerySuggested();
            }

            SetDigitalStatus(digitalInput1, out _digitalInput1Text, out _digitalInput1Tone, "DigitalInput1Text", "DigitalInput1Tone");
            SetDigitalStatus(digitalInput2, out _digitalInput2Text, out _digitalInput2Tone, "DigitalInput2Text", "DigitalInput2Tone");
            SetDigitalStatus(digitalInput3, out _digitalInput3Text, out _digitalInput3Tone, "DigitalInput3Text", "DigitalInput3Tone");
            SetDigitalStatus(digitalInput4, out _digitalInput4Text, out _digitalInput4Tone, "DigitalInput4Text", "DigitalInput4Tone");
            SetDigitalStatus(opticalSensor, out _opticalSensorText, out _opticalSensorTone, "OpticalSensorText", "OpticalSensorTone");
            SetDigitalStatus(inductiveSensor, out _inductiveSensorText, out _inductiveSensorTone, "InductiveSensorText", "InductiveSensorTone");
        }

        private void HandleDigitalInputCommands(SensorTrainerSnapshot snapshot)
        {
            if (!_hasDigitalInputCommandState)
            {
                SaveDigitalInputCommandState(snapshot);
                return;
            }

            if (snapshot.DigitalInput1 && !_lastDigitalInput1)
            {
                ExecuteControlCommand("DI1 Process Start", false);
            }

            if (snapshot.DigitalInput2 && !_lastDigitalInput2)
            {
                ExecuteControlCommand("DI2 Process Stop", false);
            }

            if (snapshot.DigitalInput3 && !_lastDigitalInput3)
            {
                ExecuteControlCommand("DI3 AI Control Start", true);
            }

            if (snapshot.DigitalInput4 && !_lastDigitalInput4)
            {
                ExecuteControlCommand("DI4 AI Control Stop", true);
            }

            SaveDigitalInputCommandState(snapshot);
        }

        private void SaveDigitalInputCommandState(SensorTrainerSnapshot snapshot)
        {
            _lastDigitalInput1 = snapshot.DigitalInput1;
            _lastDigitalInput2 = snapshot.DigitalInput2;
            _lastDigitalInput3 = snapshot.DigitalInput3;
            _lastDigitalInput4 = snapshot.DigitalInput4;
            _hasDigitalInputCommandState = true;
        }

        private void SetDigitalStatus(bool isOn, out string textField, out string toneField, string textPropertyName, string tonePropertyName)
        {
            textField = isOn ? "ON" : "OFF";
            toneField = isOn ? "Normal" : "Disabled";
            OnPropertyChanged(textPropertyName);
            OnPropertyChanged(tonePropertyName);
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
