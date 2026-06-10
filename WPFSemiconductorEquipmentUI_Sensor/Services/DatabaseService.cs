using System;
using System.Data.SQLite;
using System.IO;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class DatabaseService
    {
        private const string ApplicationFolderName = "WPFSemiconductorEquipmentUI_Sensor";
        private const string DatabaseFileName = "equipment.db";

        public DatabaseService()
        {
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DatabaseDirectory = Path.Combine(baseDirectory, ApplicationFolderName);
            DatabasePath = Path.Combine(DatabaseDirectory, DatabaseFileName);
        }

        public string DatabaseDirectory { get; private set; }
        public string DatabasePath { get; private set; }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection("Data Source=" + DatabasePath + ";Version=3;Journal Mode=WAL;BusyTimeout=3000;");
        }

        public void Initialize()
        {
            Directory.CreateDirectory(DatabaseDirectory);

            using (var connection = CreateConnection())
            {
                connection.Open();
                ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS activity_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    occurred_at TEXT NOT NULL,
    source TEXT NOT NULL,
    user_id TEXT NOT NULL,
    event TEXT NOT NULL,
    severity TEXT NOT NULL,
    saved TEXT NOT NULL DEFAULT 'YES'
);");

                ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS sensor_snapshots (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    captured_at TEXT NOT NULL,
    pressure_raw INTEGER NOT NULL,
    pressure_value REAL NOT NULL,
    vibration_raw INTEGER NOT NULL,
    vibration_value REAL NOT NULL,
    temperature_raw INTEGER NOT NULL,
    temperature_value REAL NOT NULL,
    humidity_raw INTEGER NOT NULL,
    humidity_value REAL NOT NULL,
    pressure_status TEXT NOT NULL DEFAULT 'Idle',
    vibration_status TEXT NOT NULL DEFAULT 'Idle',
    temperature_status TEXT NOT NULL DEFAULT 'Idle',
    humidity_status TEXT NOT NULL DEFAULT 'Idle',
    digital_input1 INTEGER NOT NULL,
    digital_input2 INTEGER NOT NULL,
    digital_input3 INTEGER NOT NULL,
    digital_input4 INTEGER NOT NULL,
    optical_sensor INTEGER NOT NULL,
    inductive_sensor INTEGER NOT NULL
);");

                ExecuteNonQuery(connection, @"CREATE TABLE IF NOT EXISTS app_settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);");

                // 기존 DB 마이그레이션: 센서별 상태 컬럼을 추가한다. 이미 있으면 무시.
                AddColumnIfMissing(connection, "sensor_snapshots", "pressure_status", "TEXT NOT NULL DEFAULT 'Idle'");
                AddColumnIfMissing(connection, "sensor_snapshots", "vibration_status", "TEXT NOT NULL DEFAULT 'Idle'");
                AddColumnIfMissing(connection, "sensor_snapshots", "temperature_status", "TEXT NOT NULL DEFAULT 'Idle'");
                AddColumnIfMissing(connection, "sensor_snapshots", "humidity_status", "TEXT NOT NULL DEFAULT 'Idle'");
            }
        }

        // SQLite에는 ADD COLUMN IF NOT EXISTS가 없으므로, 컬럼이 이미 존재해서 나는
        // 예외(duplicate column name)는 무시한다.
        private static void AddColumnIfMissing(SQLiteConnection connection, string table, string column, string definition)
        {
            try
            {
                ExecuteNonQuery(connection, "ALTER TABLE " + table + " ADD COLUMN " + column + " " + definition + ";");
            }
            catch (SQLiteException)
            {
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
