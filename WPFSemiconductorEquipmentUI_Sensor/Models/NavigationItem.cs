using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public class NavigationItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Title { get; set; }
        public object ViewModel { get; set; }

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
