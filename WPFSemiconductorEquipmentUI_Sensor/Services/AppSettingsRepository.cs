using System;
using System.Data.SQLite;
using System.Globalization;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class AppSettingsRepository
    {
        private readonly DatabaseService _databaseService;

        public AppSettingsRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public string GetValue(string key, string defaultValue)
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT value FROM app_settings WHERE key = @key LIMIT 1;", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    var value = command.ExecuteScalar();
                    return value == null || value == DBNull.Value ? defaultValue : Convert.ToString(value);
                }
            }
        }

        public double GetDouble(string key, double defaultValue)
        {
            var raw = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            double value;
            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            var raw = GetValue(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            int value;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : defaultValue;
        }

        public void SetValue(string key, string value)
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"INSERT OR REPLACE INTO app_settings (key, value)
VALUES (@key, @value);", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@value", value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetDouble(string key, double value)
        {
            SetValue(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetInt(string key, int value)
        {
            SetValue(key, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
