using System;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public sealed class SensorSnapshotRecord
    {
        public DateTime CapturedAt { get; set; }
        public short PressureRaw { get; set; }
        public double PressureValue { get; set; }
        public short VibrationRaw { get; set; }
        public double VibrationValue { get; set; }
        public short TemperatureRaw { get; set; }
        public double TemperatureValue { get; set; }
        public short HumidityRaw { get; set; }
        public double HumidityValue { get; set; }
        public SensorStatus PressureStatus { get; set; }
        public SensorStatus VibrationStatus { get; set; }
        public SensorStatus TemperatureStatus { get; set; }
        public SensorStatus HumidityStatus { get; set; }
        public bool DigitalInput1 { get; set; }
        public bool DigitalInput2 { get; set; }
        public bool DigitalInput3 { get; set; }
        public bool DigitalInput4 { get; set; }
        public bool OpticalSensor { get; set; }
        public bool InductiveSensor { get; set; }
    }
}
