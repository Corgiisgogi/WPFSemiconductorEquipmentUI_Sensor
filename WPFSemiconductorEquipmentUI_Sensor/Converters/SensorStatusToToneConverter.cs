namespace WPFSemiconductorEquipmentUI_Sensor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using WPFSemiconductorEquipmentUI_Sensor.Models;

    // SensorStatus(도메인 상태) → StatusBadge가 해석하는 Tone 문자열로 변환한다.
    public class SensorStatusToToneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SensorStatus))
            {
                return "Disabled";
            }

            switch ((SensorStatus)value)
            {
                case SensorStatus.Normal:
                case SensorStatus.AiCorrected:
                    return "Normal";
                case SensorStatus.Warning:
                case SensorStatus.Stale:
                    return "Warning";
                default:
                    return "Disabled";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
