using System;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class SensorTrainerSnapshot
    {
        public SensorTrainerSnapshot(
            short pressure,
            short vibration,
            short temperature,
            short humidity,
            bool digitalInput1,
            bool digitalInput2,
            bool digitalInput3,
            bool digitalInput4,
            bool opticalSensor,
            bool inductiveSensor)
        {
            Pressure = pressure;
            Vibration = vibration;
            Temperature = temperature;
            Humidity = humidity;
            DigitalInput1 = digitalInput1;
            DigitalInput2 = digitalInput2;
            DigitalInput3 = digitalInput3;
            DigitalInput4 = digitalInput4;
            OpticalSensor = opticalSensor;
            InductiveSensor = inductiveSensor;
            ReceivedAt = DateTime.Now;
        }

        public short Pressure { get; private set; }
        public short Vibration { get; private set; }
        public short Temperature { get; private set; }
        public short Humidity { get; private set; }
        public bool DigitalInput1 { get; private set; }
        public bool DigitalInput2 { get; private set; }
        public bool DigitalInput3 { get; private set; }
        public bool DigitalInput4 { get; private set; }
        public bool OpticalSensor { get; private set; }
        public bool InductiveSensor { get; private set; }
        public DateTime ReceivedAt { get; private set; }
    }
}
