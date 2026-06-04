using System.Data.SQLite;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class SensorSnapshotRepository
    {
        private readonly DatabaseService _databaseService;

        public SensorSnapshotRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void Insert(SensorSnapshotRecord snapshot)
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"INSERT INTO sensor_snapshots
(captured_at, pressure_raw, pressure_value, vibration_raw, vibration_value,
temperature_raw, temperature_value, humidity_raw, humidity_value,
digital_input1, digital_input2, digital_input3, digital_input4, optical_sensor, inductive_sensor)
VALUES
(@captured_at, @pressure_raw, @pressure_value, @vibration_raw, @vibration_value,
@temperature_raw, @temperature_value, @humidity_raw, @humidity_value,
@digital_input1, @digital_input2, @digital_input3, @digital_input4, @optical_sensor, @inductive_sensor);", connection))
                {
                    command.Parameters.AddWithValue("@captured_at", snapshot.CapturedAt.ToString("o"));
                    command.Parameters.AddWithValue("@pressure_raw", snapshot.PressureRaw);
                    command.Parameters.AddWithValue("@pressure_value", snapshot.PressureValue);
                    command.Parameters.AddWithValue("@vibration_raw", snapshot.VibrationRaw);
                    command.Parameters.AddWithValue("@vibration_value", snapshot.VibrationValue);
                    command.Parameters.AddWithValue("@temperature_raw", snapshot.TemperatureRaw);
                    command.Parameters.AddWithValue("@temperature_value", snapshot.TemperatureValue);
                    command.Parameters.AddWithValue("@humidity_raw", snapshot.HumidityRaw);
                    command.Parameters.AddWithValue("@humidity_value", snapshot.HumidityValue);
                    command.Parameters.AddWithValue("@digital_input1", ToInt(snapshot.DigitalInput1));
                    command.Parameters.AddWithValue("@digital_input2", ToInt(snapshot.DigitalInput2));
                    command.Parameters.AddWithValue("@digital_input3", ToInt(snapshot.DigitalInput3));
                    command.Parameters.AddWithValue("@digital_input4", ToInt(snapshot.DigitalInput4));
                    command.Parameters.AddWithValue("@optical_sensor", ToInt(snapshot.OpticalSensor));
                    command.Parameters.AddWithValue("@inductive_sensor", ToInt(snapshot.InductiveSensor));
                    command.ExecuteNonQuery();
                }
            }
        }

        private static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }
    }
}
