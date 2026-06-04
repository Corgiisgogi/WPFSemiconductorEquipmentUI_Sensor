namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class SensorMetric : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private string _unit;
        private string _rangeText;
        private string _badgeText;
        private string _tone;
        private double _indicatorWidth;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public string Unit
        {
            get { return _unit; }
            set { SetProperty(ref _unit, value); }
        }

        public string RangeText
        {
            get { return _rangeText; }
            set { SetProperty(ref _rangeText, value); }
        }

        public string BadgeText
        {
            get { return _badgeText; }
            set { SetProperty(ref _badgeText, value); }
        }

        public string Tone
        {
            get { return _tone; }
            set { SetProperty(ref _tone, value); }
        }

        public double IndicatorWidth
        {
            get { return _indicatorWidth; }
            set { SetProperty(ref _indicatorWidth, value); }
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

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
