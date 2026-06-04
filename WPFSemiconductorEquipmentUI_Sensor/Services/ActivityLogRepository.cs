using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class ActivityLogRepository
    {
        private readonly DatabaseService _databaseService;

        public ActivityLogRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void Insert(ActivityLogItem log)
        {
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"INSERT INTO activity_logs
(occurred_at, source, user_id, event, severity, saved)
VALUES (@occurred_at, @source, @user_id, @event, @severity, @saved);", connection))
                {
                    command.Parameters.AddWithValue("@occurred_at", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@source", log.Source ?? string.Empty);
                    command.Parameters.AddWithValue("@user_id", log.User ?? string.Empty);
                    command.Parameters.AddWithValue("@event", log.Event ?? string.Empty);
                    command.Parameters.AddWithValue("@severity", log.Severity ?? string.Empty);
                    command.Parameters.AddWithValue("@saved", "YES");
                    command.ExecuteNonQuery();
                }
            }
        }

        public ObservableCollection<ActivityLogItem> LoadRecent(int limit)
        {
            var logs = new ObservableCollection<ActivityLogItem>();

            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"SELECT occurred_at, source, user_id, event, severity, saved
FROM activity_logs
ORDER BY id DESC
LIMIT @limit;", connection))
                {
                    command.Parameters.AddWithValue("@limit", limit);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new ActivityLogItem
                            {
                                Time = FormatTime(Convert.ToString(reader["occurred_at"])),
                                Source = Convert.ToString(reader["source"]),
                                User = Convert.ToString(reader["user_id"]),
                                Event = Convert.ToString(reader["event"]),
                                Severity = Convert.ToString(reader["severity"]),
                                Saved = Convert.ToString(reader["saved"])
                            });
                        }
                    }
                }
            }

            return logs;
        }

        private static string FormatTime(string occurredAt)
        {
            DateTime parsed;
            if (DateTime.TryParse(occurredAt, out parsed))
            {
                return parsed.ToString("HH:mm:ss");
            }

            return occurredAt;
        }
    }
}
