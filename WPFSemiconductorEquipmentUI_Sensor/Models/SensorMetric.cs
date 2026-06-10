namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    // 센서의 도메인 데이터만 노출한다. 표시 문자열(포맷/배지문구/단위 조합)과 색(Tone)은
    // View 계층의 컨버터·StringFormat이 담당한다(본격 MVVM 분리).
    public class SensorMetric : INotifyPropertyChanged
    {
        private string _name;
        private string _unit;
        private double _numericValue;
        private short _rawValue;
        private SensorStatus _status;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Unit
        {
            get { return _unit; }
            set { SetProperty(ref _unit, value); }
        }

        // 보정(calibration) 후의 측정값. 표시 포맷은 View가 결정한다.
        public double NumericValue
        {
            get { return _numericValue; }
            set { SetProperty(ref _numericValue, value); }
        }

        // 장비에서 읽은 원시값.
        public short RawValue
        {
            get { return _rawValue; }
            set { SetProperty(ref _rawValue, value); }
        }

        public SensorStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
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
