using System;
using System.Runtime.CompilerServices;
using WPFSemiconductorEquipmentUI_Sensor.ViewModels;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class AppSettingsStore : ViewModelBase
    {
        public const double DefaultPressureWarningThreshold = 0.40d;
        public const double DefaultTemperatureWarningThreshold = 40d;
        public const double DefaultVibrationWarningThreshold = 7d;
        public const double DefaultHumidityWarningThreshold = 80d;
        public const int DefaultRiskWindowSeconds = 60;
        public const int DefaultAutoShutdownWarningLimit = 2;
        public const int DefaultSensorSnapshotSaveIntervalSeconds = 1;
        public const string DefaultApiBaseUrl = "http://localhost:5000";

        private readonly AppSettingsRepository _repository;
        private double _pressureWarningThreshold;
        private double _temperatureWarningThreshold;
        private double _vibrationWarningThreshold;
        private double _humidityWarningThreshold;
        private int _riskWindowSeconds;
        private int _autoShutdownWarningLimit;
        private int _sensorSnapshotSaveIntervalSeconds;
        private string _apiBaseUrl;

        public AppSettingsStore()
            : this(null)
        {
        }

        public AppSettingsStore(AppSettingsRepository repository)
        {
            _repository = repository;
            Load();
        }

        public double PressureWarningThreshold
        {
            get { return _pressureWarningThreshold; }
            set { SetProperty(ref _pressureWarningThreshold, value); }
        }

        public double TemperatureWarningThreshold
        {
            get { return _temperatureWarningThreshold; }
            set { SetProperty(ref _temperatureWarningThreshold, value); }
        }

        public double VibrationWarningThreshold
        {
            get { return _vibrationWarningThreshold; }
            set { SetProperty(ref _vibrationWarningThreshold, value); }
        }

        public double HumidityWarningThreshold
        {
            get { return _humidityWarningThreshold; }
            set { SetProperty(ref _humidityWarningThreshold, value); }
        }

        public int RiskWindowSeconds
        {
            get { return _riskWindowSeconds; }
            set { SetProperty(ref _riskWindowSeconds, Math.Max(1, value)); }
        }

        public int AutoShutdownWarningLimit
        {
            get { return _autoShutdownWarningLimit; }
            set { SetProperty(ref _autoShutdownWarningLimit, Math.Max(1, value)); }
        }

        public int SensorSnapshotSaveIntervalSeconds
        {
            get { return _sensorSnapshotSaveIntervalSeconds; }
            set { SetProperty(ref _sensorSnapshotSaveIntervalSeconds, Math.Max(1, value)); }
        }

        public string ApiBaseUrl
        {
            get { return _apiBaseUrl; }
            set { SetProperty(ref _apiBaseUrl, string.IsNullOrWhiteSpace(value) ? DefaultApiBaseUrl : value.Trim()); }
        }

        public void Load()
        {
            PressureWarningThreshold = _repository == null ? DefaultPressureWarningThreshold : NormalizePressureThreshold(_repository.GetDouble("PressureWarningThreshold", DefaultPressureWarningThreshold));
            TemperatureWarningThreshold = _repository == null ? DefaultTemperatureWarningThreshold : _repository.GetDouble("TemperatureWarningThreshold", DefaultTemperatureWarningThreshold);
            VibrationWarningThreshold = _repository == null ? DefaultVibrationWarningThreshold : _repository.GetDouble("VibrationWarningThreshold", DefaultVibrationWarningThreshold);
            HumidityWarningThreshold = _repository == null ? DefaultHumidityWarningThreshold : _repository.GetDouble("HumidityWarningThreshold", DefaultHumidityWarningThreshold);
            RiskWindowSeconds = _repository == null ? DefaultRiskWindowSeconds : _repository.GetInt("RiskWindowSeconds", DefaultRiskWindowSeconds);
            AutoShutdownWarningLimit = _repository == null ? DefaultAutoShutdownWarningLimit : _repository.GetInt("AutoShutdownWarningLimit", DefaultAutoShutdownWarningLimit);
            SensorSnapshotSaveIntervalSeconds = _repository == null ? DefaultSensorSnapshotSaveIntervalSeconds : _repository.GetInt("SensorSnapshotSaveIntervalSeconds", DefaultSensorSnapshotSaveIntervalSeconds);
            ApiBaseUrl = _repository == null ? DefaultApiBaseUrl : _repository.GetValue("ApiBaseUrl", DefaultApiBaseUrl);
        }

        private static double NormalizePressureThreshold(double value)
        {
            if (value <= 0d || value > 0.45d)
            {
                return DefaultPressureWarningThreshold;
            }

            return value;
        }

        public void Save()
        {
            if (_repository == null)
            {
                return;
            }

            _repository.SetDouble("PressureWarningThreshold", PressureWarningThreshold);
            _repository.SetDouble("TemperatureWarningThreshold", TemperatureWarningThreshold);
            _repository.SetDouble("VibrationWarningThreshold", VibrationWarningThreshold);
            _repository.SetDouble("HumidityWarningThreshold", HumidityWarningThreshold);
            _repository.SetInt("RiskWindowSeconds", RiskWindowSeconds);
            _repository.SetInt("AutoShutdownWarningLimit", AutoShutdownWarningLimit);
            _repository.SetInt("SensorSnapshotSaveIntervalSeconds", SensorSnapshotSaveIntervalSeconds);
            _repository.SetValue("ApiBaseUrl", ApiBaseUrl);
        }

        public void ResetToDefaults()
        {
            PressureWarningThreshold = DefaultPressureWarningThreshold;
            TemperatureWarningThreshold = DefaultTemperatureWarningThreshold;
            VibrationWarningThreshold = DefaultVibrationWarningThreshold;
            HumidityWarningThreshold = DefaultHumidityWarningThreshold;
            RiskWindowSeconds = DefaultRiskWindowSeconds;
            AutoShutdownWarningLimit = DefaultAutoShutdownWarningLimit;
            SensorSnapshotSaveIntervalSeconds = DefaultSensorSnapshotSaveIntervalSeconds;
            ApiBaseUrl = DefaultApiBaseUrl;
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
