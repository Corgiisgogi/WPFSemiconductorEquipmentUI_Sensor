using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public class NavigationItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isVisible = true;

        public string Title { get; set; }
        public object ViewModel { get; set; }
        public bool RequiresAdmin { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                OnPropertyChanged();
                OnPropertyChanged("Visibility");
            }
        }

        public Visibility Visibility
        {
            get { return IsVisible ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
