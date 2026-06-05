using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class SettingsViewModel : ScreenViewModelBase
    {
        private readonly AppSettingsStore _settings;
        private string _settingsStatusText;
        private string _settingsStatusTone;
        private string _pressureWarningThresholdText;
        private string _temperatureWarningThresholdText;
        private string _vibrationWarningThresholdText;
        private string _humidityWarningThresholdText;

        public SettingsViewModel()
            : this(new AppSettingsStore(), null)
        {
        }

        public SettingsViewModel(AppSettingsStore settings, DatabaseService databaseService)
        {
            _settings = settings;
            DatabasePath = databaseService == null ? "LocalAppData/equipment.db" : databaseService.DatabasePath;
            Title = "시스템 설정";
            Description = "로컬 콘솔의 연결, 인증, 위험 규칙, 로그 저장 설정을 관리합니다.";
            RefreshThresholdText();
            SettingsStatusText = "DB 연결";
            SettingsStatusTone = "Normal";
            SaveCommand = new RelayCommand(parameter => SaveSettings());
            ResetCommand = new RelayCommand(parameter => ResetSettings());
            TestCommand = new RelayCommand(parameter => TestSettings());
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public string DatabasePath { get; private set; }

        public string ApiBaseUrl
        {
            get { return _settings.ApiBaseUrl; }
            set
            {
                if (object.Equals(_settings.ApiBaseUrl, value))
                {
                    return;
                }

                _settings.ApiBaseUrl = value;
                OnPropertyChanged();
            }
        }

        public string SettingsStatusText
        {
            get { return _settingsStatusText; }
            private set { SetProperty(ref _settingsStatusText, value); }
        }

        public string SettingsStatusTone
        {
            get { return _settingsStatusTone; }
            private set { SetProperty(ref _settingsStatusTone, value); }
        }

        public string PressureWarningThresholdText
        {
            get { return _pressureWarningThresholdText; }
            set
            {
                if (object.Equals(_pressureWarningThresholdText, value))
                {
                    return;
                }

                _pressureWarningThresholdText = value;
                ApplyDoubleSetting(value, parsed => _settings.PressureWarningThreshold = parsed, "압력 경고 기준");
                OnPropertyChanged();
            }
        }

        public string TemperatureWarningThresholdText
        {
            get { return _temperatureWarningThresholdText; }
            set
            {
                if (object.Equals(_temperatureWarningThresholdText, value))
                {
                    return;
                }

                _temperatureWarningThresholdText = value;
                ApplyDoubleSetting(value, parsed => _settings.TemperatureWarningThreshold = parsed, "온도 경고 기준");
                OnPropertyChanged();
            }
        }

        public string VibrationWarningThresholdText
        {
            get { return _vibrationWarningThresholdText; }
            set
            {
                if (object.Equals(_vibrationWarningThresholdText, value))
                {
                    return;
                }

                _vibrationWarningThresholdText = value;
                ApplyDoubleSetting(value, parsed => _settings.VibrationWarningThreshold = parsed, "진동 경고 기준");
                OnPropertyChanged();
            }
        }

        public string HumidityWarningThresholdText
        {
            get { return _humidityWarningThresholdText; }
            set
            {
                if (object.Equals(_humidityWarningThresholdText, value))
                {
                    return;
                }

                _humidityWarningThresholdText = value;
                ApplyDoubleSetting(value, parsed => _settings.HumidityWarningThreshold = parsed, "습도 경고 기준");
                OnPropertyChanged();
            }
        }

        public int RiskWindowSeconds
        {
            get { return _settings.RiskWindowSeconds; }
            set
            {
                if (_settings.RiskWindowSeconds == value)
                {
                    return;
                }

                _settings.RiskWindowSeconds = value;
                OnPropertyChanged();
            }
        }

        public int AutoShutdownWarningLimit
        {
            get { return _settings.AutoShutdownWarningLimit; }
            set
            {
                if (_settings.AutoShutdownWarningLimit == value)
                {
                    return;
                }

                _settings.AutoShutdownWarningLimit = value;
                OnPropertyChanged();
            }
        }

        public int SensorSnapshotSaveIntervalSeconds
        {
            get { return _settings.SensorSnapshotSaveIntervalSeconds; }
            set
            {
                if (_settings.SensorSnapshotSaveIntervalSeconds == value)
                {
                    return;
                }

                _settings.SensorSnapshotSaveIntervalSeconds = value;
                OnPropertyChanged();
            }
        }

        private void RefreshThresholdText()
        {
            _pressureWarningThresholdText = FormatDouble(_settings.PressureWarningThreshold);
            _temperatureWarningThresholdText = FormatDouble(_settings.TemperatureWarningThreshold);
            _vibrationWarningThresholdText = FormatDouble(_settings.VibrationWarningThreshold);
            _humidityWarningThresholdText = FormatDouble(_settings.HumidityWarningThreshold);
        }

        private void ApplyDoubleSetting(string text, Action<double> apply, string label)
        {
            double parsed;
            if (TryParseDouble(text, out parsed))
            {
                apply(parsed);
                SettingsStatusText = "수정 중";
                SettingsStatusTone = "Blue";
                return;
            }

            SettingsStatusText = "입력 오류";
            SettingsStatusTone = "Danger";
            Description = label + "은(는) 소수 입력이 필요합니다. 예: 0.20, 0.8, 7.5";
            OnPropertyChanged("Description");
        }

        private static bool TryParseDouble(string text, out double value)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                value = 0d;
                return false;
            }

            var normalized = text.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void SaveSettings()
        {
            try
            {
                _settings.Save();
                RefreshThresholdText();
                SettingsStatusText = "저장됨";
                SettingsStatusTone = "Normal";
            }
            catch (Exception ex)
            {
                SettingsStatusText = "저장 오류";
                SettingsStatusTone = "Danger";
                Description = "설정 저장 실패: " + ex.Message;
                OnPropertyChanged("Description");
            }
        }

        private void ResetSettings()
        {
            _settings.ResetToDefaults();
            RefreshThresholdText();
            OnPropertyChanged("PressureWarningThresholdText");
            OnPropertyChanged("TemperatureWarningThresholdText");
            OnPropertyChanged("VibrationWarningThresholdText");
            OnPropertyChanged("HumidityWarningThresholdText");
            OnPropertyChanged("RiskWindowSeconds");
            OnPropertyChanged("AutoShutdownWarningLimit");
            OnPropertyChanged("SensorSnapshotSaveIntervalSeconds");
            OnPropertyChanged("ApiBaseUrl");
            SettingsStatusText = "기본값";
            SettingsStatusTone = "Warning";
        }

        private void TestSettings()
        {
            SettingsStatusText = "준비";
            SettingsStatusTone = "Blue";
            Description = "현재 설정은 콘솔 AI 위험 규칙에 즉시 반영됩니다. SQLite에 영구 저장하려면 저장을 누르세요.";
            OnPropertyChanged("Description");
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
