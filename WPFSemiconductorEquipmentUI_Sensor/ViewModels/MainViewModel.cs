using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private object _currentViewModel;
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
            var activityLogStore = new ActivityLogStore(activityLogRepository);
            var auth = new LoginViewModel(Session, authService);
            var console = new ConsoleViewModel(Session, activityLogStore, sensorSnapshotRepository, appSettingsStore);
            var logs = new LogsViewModel(activityLogStore);
            var settings = new SettingsViewModel(appSettingsStore, databaseService);

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Title = "Auth", ViewModel = auth, IsSelected = true },
                new NavigationItem { Title = "Console", ViewModel = console },
                new NavigationItem { Title = "Logs", ViewModel = logs },
                new NavigationItem { Title = "Settings", ViewModel = settings }
            };

            PendingViewModel = new PendingViewModel();
            CurrentViewModel = auth;
            NavigateCommand = new RelayCommand(Navigate);
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; private set; }
        public PendingViewModel PendingViewModel { get; private set; }
        public ICommand NavigateCommand { get; private set; }
        public UserSession Session { get; private set; }

        public object CurrentViewModel
        {
            get { return _currentViewModel; }
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        private void Navigate(object parameter)
        {
            var item = parameter as NavigationItem;
            if (item == null)
            {
                return;
            }

            foreach (var navigationItem in NavigationItems)
            {
                navigationItem.IsSelected = false;
            }

            item.IsSelected = true;
            CurrentViewModel = item.ViewModel;
            OnPropertyChanged("NavigationItems");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
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
