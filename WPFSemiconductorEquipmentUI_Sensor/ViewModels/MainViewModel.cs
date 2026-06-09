using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        // Development/test switch: allows opening Settings before an Admin login so the Flask API URL
        // can be changed between localhost and a classroom server. Set to false for final operation.
        private const bool AllowSettingsWithoutAdminForApiSetup = true;
        private object _currentViewModel;
        private NavigationItem _authNavigationItem;
        private NavigationItem _settingsNavigationItem;
        private ConsoleViewModel _consoleViewModel;
        private readonly DispatcherTimer _clockTimer;
        private string _currentTimeText;
        private bool _disposed;

        public MainViewModel()
        {
            Session = new UserSession();

            var databaseService = new DatabaseService();
            databaseService.Initialize();
            var activityLogRepository = new ActivityLogRepository(databaseService);
            var sensorSnapshotRepository = new SensorSnapshotRepository(databaseService);
            var appSettingsRepository = new AppSettingsRepository(databaseService);
            var appSettingsStore = new AppSettingsStore(appSettingsRepository);
            var authService = new FlaskAuthService(appSettingsStore);
            var remoteTelemetryService = new FlaskTelemetryService(appSettingsStore);
            var activityLogStore = new ActivityLogStore(activityLogRepository, remoteTelemetryService);
            var auth = new LoginViewModel(Session, authService);
            var console = new ConsoleViewModel(Session, activityLogStore, sensorSnapshotRepository, appSettingsStore, remoteTelemetryService);
            _consoleViewModel = console;
            _consoleViewModel.PropertyChanged += OnConsolePropertyChanged;
            var logs = new LogsViewModel(activityLogStore, activityLogRepository, sensorSnapshotRepository);
            var settings = new SettingsViewModel(appSettingsStore, databaseService);

            _authNavigationItem = new NavigationItem { Title = "AUTH", ViewModel = auth };
            _settingsNavigationItem = new NavigationItem
            {
                Title = "SETTINGS",
                ViewModel = settings,
                RequiresAdmin = !AllowSettingsWithoutAdminForApiSetup,
                IsVisible = AllowSettingsWithoutAdminForApiSetup || Session.IsAdmin
            };

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Title = "MAIN", ViewModel = console, IsSelected = true },
                _authNavigationItem,
                new NavigationItem { Title = "LOG", ViewModel = logs },
                _settingsNavigationItem
            };

            PendingViewModel = new PendingViewModel();
            CurrentViewModel = console;
            NavigateCommand = new RelayCommand(Navigate);
            CurrentTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += OnClockTimerTick;
            _clockTimer.Start();
            Session.PropertyChanged += OnSessionPropertyChanged;
            UpdateAdminNavigationAccess();
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; private set; }
        public PendingViewModel PendingViewModel { get; private set; }
        public ICommand NavigateCommand { get; private set; }
        public UserSession Session { get; private set; }

        public string CurrentTimeText
        {
            get { return _currentTimeText; }
            private set
            {
                if (_currentTimeText == value)
                {
                    return;
                }

                _currentTimeText = value;
                OnPropertyChanged();
            }
        }

        public string AlertSummaryText
        {
            get
            {
                if (_consoleViewModel == null)
                {
                    return "경고 0";
                }

                if (string.Equals(_consoleViewModel.RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase))
                {
                    return "위험 1";
                }

                if (string.Equals(_consoleViewModel.RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase))
                {
                    return "경고 1";
                }

                return "경고 0";
            }
        }

        public string AlertSummaryTone
        {
            get
            {
                if (_consoleViewModel == null)
                {
                    return "Normal";
                }

                if (string.Equals(_consoleViewModel.RiskStatusTone, "Danger", StringComparison.OrdinalIgnoreCase))
                {
                    return "Danger";
                }

                if (string.Equals(_consoleViewModel.RiskStatusTone, "Warning", StringComparison.OrdinalIgnoreCase))
                {
                    return "Warning";
                }

                return "Normal";
            }
        }

        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        private void OnConsolePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RiskStatusText" || e.PropertyName == "RiskStatusTone")
            {
                OnPropertyChanged("AlertSummaryText");
                OnPropertyChanged("AlertSummaryTone");
            }
        }

        private void OnClockTimerTick(object sender, EventArgs e)
        {
            CurrentTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void Navigate(object parameter)
        {
            var item = parameter as NavigationItem;
            if (item == null)
            {
                return;
            }

            if (item.RequiresAdmin && !Session.IsAdmin && !AllowSettingsWithoutAdminForApiSetup)
            {
                NavigateToAuth();
                return;
            }

            foreach (var navigationItem in NavigationItems)
            {
                navigationItem.IsSelected = false;
            }

            item.IsSelected = true;
            var logsViewModel = item.ViewModel as LogsViewModel;
            if (logsViewModel != null)
            {
                logsViewModel.Refresh();
            }

            CurrentViewModel = item.ViewModel;
            OnPropertyChanged("NavigationItems");
        }

        private void OnSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsAdmin" || e.PropertyName == "IsApproved" || e.PropertyName == "RoleText")
            {
                UpdateAdminNavigationAccess();
            }
        }

        private void UpdateAdminNavigationAccess()
        {
            if (_settingsNavigationItem == null)
            {
                return;
            }

            _settingsNavigationItem.IsVisible = AllowSettingsWithoutAdminForApiSetup || Session.IsAdmin;
            if (!AllowSettingsWithoutAdminForApiSetup && !Session.IsAdmin && object.ReferenceEquals(CurrentViewModel, _settingsNavigationItem.ViewModel))
            {
                NavigateToAuth();
            }
        }

        private void NavigateToAuth()
        {
            foreach (var navigationItem in NavigationItems)
            {
                navigationItem.IsSelected = false;
            }

            if (_authNavigationItem != null)
            {
                _authNavigationItem.IsSelected = true;
                CurrentViewModel = _authNavigationItem.ViewModel;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Session.PropertyChanged -= OnSessionPropertyChanged;
            if (_consoleViewModel != null)
            {
                _consoleViewModel.PropertyChanged -= OnConsolePropertyChanged;
            }

            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer.Tick -= OnClockTimerTick;
            }

            foreach (var item in NavigationItems)
            {
                var disposable = item.ViewModel as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
