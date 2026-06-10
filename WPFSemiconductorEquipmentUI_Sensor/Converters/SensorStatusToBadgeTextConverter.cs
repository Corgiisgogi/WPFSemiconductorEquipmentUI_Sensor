namespace WPFSemiconductorEquipmentUI_Sensor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using WPFSemiconductorEquipmentUI_Sensor.Models;

    // SensorStatus(도메인 상태) → 화면에 표시할 한글 배지 문구로 변환한다.
    public class SensorStatusToBadgeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SensorStatus))
            {
                return "대기";
            }

            switch ((SensorStatus)value)
            {
                case SensorStatus.Normal:
                    return "정상";
                case SensorStatus.Warning:
                    return "경고";
                case SensorStatus.Stale:
                    return "정지";
                case SensorStatus.AiCorrected:
                    return "AI 보정";
                default:
                    return "대기";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
