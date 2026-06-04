using System.Collections.ObjectModel;
using System;
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
        private const int CommandPulseMilliseconds = 200;
        private const int ProcessStartOutputBit = 1;
        private const int ProcessStopOutputBit = 2;
        private const int AiControlStartOutputBit = 3;
        private const int AiControlStopOutputBit = 4;

        private readonly ITrainerClient _trainerClient;
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
        private string _connectionStatusText;
        private string _connectionStatusTone;
        private string _lastUpdateText;
        private string _etherCatStatusText;
        private string _etherCatStatusTone;
        private string _summaryBadgeText;
        private string _summaryTone;
        private string _summaryText;
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
            : this(new UserSession(), new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(UserSession session)
            : this(session, new AdsSensorTrainerClient())
        {
        }

        public ConsoleViewModel(ITrainerClient trainerClient)
            : this(new UserSession(), trainerClient)
        {
        }

        public ConsoleViewModel(UserSession session, ITrainerClient trainerClient)
        {
            _session = session;
            _trainerClient = trainerClient;
            _session.PropertyChanged += OnSessionPropertyChanged;

            Title = "WPF Equipment Control Console";
            Description = "Desktop interface for field control, sensor review, and activity monitoring.";

            Sensors = new ObservableCollection<SensorMetric>
            {
                new SensorMetric { Name = "Pressure", Value = "--", Unit = "bar", RangeText = "Raw: --", BadgeText = "WAIT", Tone = "Disabled", IndicatorWidth = 0 },
                new SensorMetric { Name = "Vibration", Value = "--", Unit = "level", RangeText = "Raw: --", BadgeText = "WAIT", Tone = "Disabled", IndicatorWidth = 0 },
                new SensorMetric { Name = "Temperature", Value = "--", Unit = "C", RangeText = "Raw: --", BadgeText = "WAIT", Tone = "Disabled", IndicatorWidth = 0 },
                new SensorMetric { Name = "Humidity", Value = "--", Unit = "%", RangeText = "Raw: --", BadgeText = "WAIT", Tone = "Disabled", IndicatorWidth = 0 }
            };

            ActivityLogs = SampleData.CreateLogs();

            ConnectionStatusText = "DISCONNECTED";
            ConnectionStatusTone = "Disabled";
            LastUpdateText = "LAST UPDATE --:--:--";
            EtherCatStatusText = "DISCONNECTED";
            EtherCatStatusTone = "Disabled";
            SummaryBadgeText = "WAITING";
            SummaryTone = "Disabled";
            SummaryText = "Waiting for TwinCAT ADS connection. Last known sensor values will remain visible if reads fail.";
            SetDigitalInputs(false, false, false, false, false, false);
            ProcessStartCommand = new RelayCommand(parameter => ExecuteDigitalOutputCommand(ProcessStartOutputBit, "DI1 Process Start", false), parameter => CanExecuteApprovedCommand());
            ProcessStopCommand = new RelayCommand(parameter => ExecuteDigitalOutputCommand(ProcessStopOutputBit, "DI2 Process Stop", false), parameter => CanExecuteApprovedCommand());
            AiControlStartCommand = new RelayCommand(parameter => ExecuteDigitalOutputCommand(AiControlStartOutputBit, "DI3 AI Control Start", true), parameter => CanExecuteAdminCommand());
            AiControlStopCommand = new RelayCommand(parameter => ExecuteDigitalOutputCommand(AiControlStopOutputBit, "DI4 AI Control Stop", true), parameter => CanExecuteAdminCommand());

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

        public string AiControlAccessText
        {
            get { return _session.IsAdmin ? "ADMIN READY" : "ADMIN REQUIRED"; }
        }

        public string AiControlAccessTone
        {
            get { return _session.IsAdmin ? "Normal" : "Danger"; }
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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool CanExecuteApprovedCommand()
        {
            return !_disposed && _session.IsApproved;
        }

        private bool CanExecuteAdminCommand()
        {
            return !_disposed && _session.IsAdmin;
        }

        private void ExecuteDigitalOutputCommand(int outputBit, string commandName, bool adminOnly)
        {
            if (adminOnly && !_session.IsAdmin)
            {
                AddActivityLog("Control", commandName + " blocked - admin required", "RISK");
                return;
            }

            if (!_session.IsApproved)
            {
                AddActivityLog("Control", commandName + " blocked - approved login required", "RISK");
                return;
            }

            try
            {
                _trainerClient.PulseDigitalOutput(outputBit, CommandPulseMilliseconds);
                AddActivityLog("Control", commandName + " command sent", adminOnly ? "WARN" : "INFO");
                SummaryBadgeText = "COMMAND SENT";
                SummaryTone = adminOnly ? "Warning" : "Normal";
                SummaryText = commandName + " was pulsed on GVL.NX_OD5121 bit " + outputBit + " for " + CommandPulseMilliseconds + " ms.";
            }
            catch (Exception ex)
            {
                AddActivityLog("Control", commandName + " failed: " + ex.Message, "RISK");
                SummaryBadgeText = "COMMAND ERROR";
                SummaryTone = "Danger";
                SummaryText = commandName + " could not be sent. " + ex.Message;
            }
        }

        private void AddActivityLog(string source, string eventText, string severity)
        {
            ActivityLogs.Insert(0, new ActivityLogItem
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Source = source,
                User = _session.UserId,
                Event = eventText,
                Severity = severity,
                Saved = "NO"
            });
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

            UpdateSensor(Sensors[0], snapshot.Pressure, CalibratePressure, "0.00", 0d, 1d);
            UpdateSensor(Sensors[1], snapshot.Vibration, CalibrateVibration, "0.0", 0d, 10d);
            UpdateSensor(Sensors[2], snapshot.Temperature, CalibrateTemperature, "0.0", 0d, 60d);
            UpdateSensor(Sensors[3], snapshot.Humidity, CalibrateHumidity, "0.0", 0d, 100d);

            SetDigitalInputs(
                snapshot.DigitalInput1,
                snapshot.DigitalInput2,
                snapshot.DigitalInput3,
                snapshot.DigitalInput4,
                snapshot.OpticalSensor,
                snapshot.InductiveSensor);
            HandleDigitalInputCommands(snapshot);

            ConnectionStatusText = "ADS READ OK";
            ConnectionStatusTone = "Normal";
            LastUpdateText = "READ #" + _successfulReadCount + " " + snapshot.ReceivedAt.ToString("HH:mm:ss");
            EtherCatStatusText = "READY";
            EtherCatStatusTone = "Normal";
            SummaryBadgeText = "PLC LINK";
            SummaryTone = "Normal";
            SummaryText = "TwinCAT ADS is readable. Sensor power state is not verified; displayed values are the latest PLC raw inputs from GVL.NX_AD4203.";
        }

        private void ApplyStaleSnapshot(SensorTrainerSnapshot snapshot)
        {
            if (_disposed)
            {
                return;
            }

            _successfulReadCount++;

            UpdateSensor(Sensors[0], snapshot.Pressure, CalibratePressure, "0.00", 0d, 1d);
            UpdateSensor(Sensors[1], snapshot.Vibration, CalibrateVibration, "0.0", 0d, 10d);
            UpdateSensor(Sensors[2], snapshot.Temperature, CalibrateTemperature, "0.0", 0d, 60d);
            UpdateSensor(Sensors[3], snapshot.Humidity, CalibrateHumidity, "0.0", 0d, 100d);
            SetSensorStale(Sensors[0]);
            SetSensorStale(Sensors[1]);
            SetSensorStale(Sensors[2]);
            SetSensorStale(Sensors[3]);

            SetDigitalInputs(
                snapshot.DigitalInput1,
                snapshot.DigitalInput2,
                snapshot.DigitalInput3,
                snapshot.DigitalInput4,
                snapshot.OpticalSensor,
                snapshot.InductiveSensor);
            HandleDigitalInputCommands(snapshot);

            ConnectionStatusText = "STALE";
            ConnectionStatusTone = "Warning";
            LastUpdateText = "READ #" + _successfulReadCount + " " + snapshot.ReceivedAt.ToString("HH:mm:ss");
            EtherCatStatusText = "STALE";
            EtherCatStatusTone = "Warning";
            SummaryBadgeText = "STALE";
            SummaryTone = "Warning";
            SummaryText = "PLC raw sensor values have not changed for about 3 seconds. ADS is readable, but sensor input updates appear stopped.";
        }

        private void ApplyReadFailure(Exception ex)
        {
            if (_disposed)
            {
                return;
            }

            ConnectionStatusText = "READ ERROR";
            ConnectionStatusTone = "Danger";
            EtherCatStatusText = "DISCONNECTED";
            EtherCatStatusTone = "Danger";
            SummaryBadgeText = "READ ERROR";
            SummaryTone = "Danger";
            SummaryText = "Unable to read TwinCAT ADS data. Check TwinCAT runtime, ADS route, port 851, and GVL.NX_* variables. " + ex.Message;
        }

        private void SetSensorStale(SensorMetric sensor)
        {
            sensor.BadgeText = "STALE";
            sensor.Tone = "Warning";
        }

        private void UpdateSensor(
            SensorMetric sensor,
            short rawValue,
            Func<short, double> calibration,
            string format,
            double minimum,
            double maximum)
        {
            var calibratedValue = calibration(rawValue);

            sensor.Value = calibratedValue.ToString(format);
            sensor.RangeText = "Raw: " + rawValue;
            sensor.BadgeText = "LIVE";
            sensor.Tone = "Normal";
            sensor.IndicatorWidth = CalculateIndicatorWidth(calibratedValue, minimum, maximum);
        }

        private static double CalculateIndicatorWidth(double value, double minimum, double maximum)
        {
            if (maximum <= minimum)
            {
                return 8d;
            }

            var normalized = (value - minimum) / (maximum - minimum);
            normalized = Math.Max(0d, Math.Min(1d, normalized));
            return Math.Max(8d, normalized * 220d);
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
                ExecuteDigitalOutputCommand(ProcessStartOutputBit, "DI1 Process Start", false);
            }

            if (snapshot.DigitalInput2 && !_lastDigitalInput2)
            {
                ExecuteDigitalOutputCommand(ProcessStopOutputBit, "DI2 Process Stop", false);
            }

            if (snapshot.DigitalInput3 && !_lastDigitalInput3)
            {
                ExecuteDigitalOutputCommand(AiControlStartOutputBit, "DI3 AI Control Start", true);
            }

            if (snapshot.DigitalInput4 && !_lastDigitalInput4)
            {
                ExecuteDigitalOutputCommand(AiControlStopOutputBit, "DI4 AI Control Stop", true);
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
