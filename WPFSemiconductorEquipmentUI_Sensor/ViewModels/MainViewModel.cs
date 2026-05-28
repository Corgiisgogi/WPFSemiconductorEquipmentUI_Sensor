using System.Collections.ObjectModel;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentViewModel;

        public MainViewModel()
        {
            var auth = new LoginViewModel();
            var console = new ConsoleViewModel();
            var logs = new LogsViewModel();
            var settings = new SettingsViewModel();

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Title = "Auth", ViewModel = auth },
                new NavigationItem { Title = "Console", ViewModel = console, IsSelected = true },
                new NavigationItem { Title = "Logs", ViewModel = logs },
                new NavigationItem { Title = "Settings", ViewModel = settings }
            };

            PendingViewModel = new PendingViewModel();
            CurrentViewModel = console;
            NavigateCommand = new RelayCommand(Navigate);
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; private set; }
        public PendingViewModel PendingViewModel { get; private set; }
        public ICommand NavigateCommand { get; private set; }

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
    }
}
