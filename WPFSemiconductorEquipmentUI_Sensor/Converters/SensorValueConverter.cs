namespace WPFSemiconductorEquipmentUI_Sensor.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using WPFSemiconductorEquipmentUI_Sensor.Models;

    // (NumericValue, Status, Unit)를 "값 단위" 표시 문자열로 조합한다.
    // 숫자 포맷은 ConverterParameter로 바인딩마다 지정(센서별 소수 자릿수 차이).
    // 아직 읽기 전(Idle)이면 값 대신 "--"를 표시한다.
    public class SensorValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return "--";
            }

            var status = values[1] is SensorStatus ? (SensorStatus)values[1] : SensorStatus.Idle;
            var unit = values[2] as string ?? string.Empty;

            if (status == SensorStatus.Idle || !(values[0] is double))
            {
                return ("-- " + unit).TrimEnd();
            }

            var format = parameter as string;
            if (string.IsNullOrEmpty(format))
            {
                format = "0.0";
            }

            var text = ((double)values[0]).ToString(format, culture);
            return (text + " " + unit).TrimEnd();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
