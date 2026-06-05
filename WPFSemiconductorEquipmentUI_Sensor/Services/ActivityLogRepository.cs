using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Text;
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

        public long Insert(ActivityLogItem log)
        {
            var occurredAt = log.OccurredAt == DateTime.MinValue ? DateTime.Now : log.OccurredAt;

            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"INSERT INTO activity_logs
(occurred_at, source, user_id, event, severity, saved)
VALUES (@occurred_at, @source, @user_id, @event, @severity, @saved);
SELECT last_insert_rowid();", connection))
                {
                    command.Parameters.AddWithValue("@occurred_at", occurredAt.ToString("o"));
                    command.Parameters.AddWithValue("@source", log.Source ?? string.Empty);
                    command.Parameters.AddWithValue("@user_id", log.User ?? string.Empty);
                    command.Parameters.AddWithValue("@event", log.Event ?? string.Empty);
                    command.Parameters.AddWithValue("@severity", log.Severity ?? string.Empty);
                    command.Parameters.AddWithValue("@saved", "YES");
                    var id = Convert.ToInt64(command.ExecuteScalar());
                    log.Id = id;
                    log.OccurredAt = occurredAt;
                    log.Time = occurredAt.ToString("HH:mm:ss");
                    return id;
                }
            }
        }

        public ObservableCollection<ActivityLogItem> LoadRecent(int limit)
        {
            return Search(new ActivityLogSearchCriteria { Limit = limit });
        }

        public ObservableCollection<ActivityLogItem> Search(ActivityLogSearchCriteria criteria)
        {
            criteria = criteria ?? new ActivityLogSearchCriteria();
            var logs = new ObservableCollection<ActivityLogItem>();

            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var sql = new StringBuilder();
                    sql.AppendLine("SELECT id, occurred_at, source, user_id, event, severity, saved");
                    sql.AppendLine("FROM activity_logs");
                    AppendWhere(sql, command, criteria);
                    sql.AppendLine("ORDER BY id DESC");
                    sql.AppendLine("LIMIT @limit;");
                    command.Parameters.AddWithValue("@limit", criteria.Limit <= 0 ? 300 : criteria.Limit);
                    command.CommandText = sql.ToString();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(ReadLog(reader));
                        }
                    }
                }
            }

            return logs;
        }

        public ActivityLogSummary GetSummary(ActivityLogSearchCriteria criteria)
        {
            criteria = criteria ?? new ActivityLogSearchCriteria();
            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                return new ActivityLogSummary
                {
                    TotalCount = Count(connection, criteria, null),
                    WarningCount = Count(connection, criteria, "WARN"),
                    RiskCount = Count(connection, criteria, "RISK"),
                    LastShutdownTimeText = LoadLastShutdownTimeText(connection, criteria)
                };
            }
        }

        public ObservableCollection<string> LoadDistinctValues(string columnName, string allText)
        {
            var values = new ObservableCollection<string>();
            values.Add(allText);

            if (columnName != "user_id" && columnName != "source" && columnName != "severity")
            {
                return values;
            }

            using (var connection = _databaseService.CreateConnection())
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT DISTINCT " + columnName + " FROM activity_logs WHERE " + columnName + " <> '' ORDER BY " + columnName + ";", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        values.Add(Convert.ToString(reader[0]));
                    }
                }
            }

            return values;
        }

        private static int Count(SQLiteConnection connection, ActivityLogSearchCriteria criteria, string severity)
        {
            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder();
                sql.AppendLine("SELECT COUNT(*) FROM activity_logs");
                AppendWhere(sql, command, criteria);
                if (!string.IsNullOrWhiteSpace(severity))
                {
                    sql.AppendLine(sql.ToString().IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0 ? "AND severity = @countSeverity" : "WHERE severity = @countSeverity");
                    command.Parameters.AddWithValue("@countSeverity", severity);
                }

                command.CommandText = sql.ToString();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static string LoadLastShutdownTimeText(SQLiteConnection connection, ActivityLogSearchCriteria criteria)
        {
            using (var command = connection.CreateCommand())
            {
                var sql = new StringBuilder();
                sql.AppendLine("SELECT occurred_at FROM activity_logs");
                AppendWhere(sql, command, criteria);
                sql.AppendLine(sql.ToString().IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "AND (event LIKE @shutdownKeyword OR event LIKE @stopKeyword)"
                    : "WHERE (event LIKE @shutdownKeyword OR event LIKE @stopKeyword)");
                sql.AppendLine("ORDER BY id DESC LIMIT 1;");
                command.Parameters.AddWithValue("@shutdownKeyword", "%shutdown%");
                command.Parameters.AddWithValue("@stopKeyword", "%stop%");
                command.CommandText = sql.ToString();
                var value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    return "--:--";
                }

                return FormatTime(Convert.ToString(value));
            }
        }

        private static void AppendWhere(StringBuilder sql, SQLiteCommand command, ActivityLogSearchCriteria criteria)
        {
            var clauses = new List<string>();
            if (criteria.FromDate.HasValue)
            {
                clauses.Add("occurred_at >= @fromDate");
                command.Parameters.AddWithValue("@fromDate", criteria.FromDate.Value.Date.ToString("o"));
            }

            if (criteria.ToDate.HasValue)
            {
                clauses.Add("occurred_at < @toDateExclusive");
                command.Parameters.AddWithValue("@toDateExclusive", criteria.ToDate.Value.Date.AddDays(1).ToString("o"));
            }

            if (!IsAll(criteria.User))
            {
                clauses.Add("user_id = @user");
                command.Parameters.AddWithValue("@user", criteria.User);
            }

            if (!IsAll(criteria.Source))
            {
                clauses.Add("source = @source");
                command.Parameters.AddWithValue("@source", criteria.Source);
            }

            if (!IsAll(criteria.Severity))
            {
                clauses.Add("severity = @severity");
                command.Parameters.AddWithValue("@severity", criteria.Severity);
            }

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                clauses.Add("(source LIKE @keyword OR user_id LIKE @keyword OR event LIKE @keyword OR severity LIKE @keyword)");
                command.Parameters.AddWithValue("@keyword", "%" + criteria.Keyword.Trim() + "%");
            }

            if (clauses.Count > 0)
            {
                sql.AppendLine("WHERE " + string.Join(" AND ", clauses));
            }
        }

        private static ActivityLogItem ReadLog(SQLiteDataReader reader)
        {
            var occurredAtText = Convert.ToString(reader["occurred_at"]);
            var occurredAt = ParseDateTime(occurredAtText);
            return new ActivityLogItem
            {
                Id = Convert.ToInt64(reader["id"]),
                OccurredAt = occurredAt,
                Time = occurredAt == DateTime.MinValue ? occurredAtText : occurredAt.ToString("HH:mm:ss"),
                Source = Convert.ToString(reader["source"]),
                User = Convert.ToString(reader["user_id"]),
                Event = TranslateEvent(Convert.ToString(reader["event"])),
                Severity = Convert.ToString(reader["severity"]),
                Saved = Convert.ToString(reader["saved"])
            };
        }

        private static string TranslateEvent(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var translated = text
                .Replace("Pressure", "압력")
                .Replace("Vibration", "진동")
                .Replace("Temperature", "온도")
                .Replace("Humidity", "습도")
                .Replace(" exceeded. Risk count ", " 기준 초과. 위험 카운트 ")
                .Replace(" within ", " / 감시 시간 ")
                .Replace("DI1 Process Start command accepted", "DI1 공정 시작 명령 승인")
                .Replace("DI2 Process Stop command accepted", "DI2 공정 정지 명령 승인")
                .Replace("DI3 AI Control Start command accepted", "DI3 AI 제어 시작 명령 승인")
                .Replace("DI4 AI Control Stop command accepted", "DI4 AI 제어 정지 명령 승인")
                .Replace(" blocked - admin required", " 차단 - 관리자 권한 필요")
                .Replace(" blocked - approved login required", " 차단 - 승인된 로그인 필요")
                .Replace("Force Shutdown blocked - admin or AI risk condition required", "강제 정지 차단 - 관리자 또는 AI 위험 조건 필요")
                .Replace("Force Shutdown blocked - approved login required", "강제 정지 차단 - 승인된 로그인 필요")
                .Replace("Auto Force Shutdown event raised", "자동 강제 정지 이벤트 발생")
                .Replace("Force Shutdown event raised", "강제 정지 이벤트 발생")
                .Replace("Sensor snapshot save failed", "센서 스냅샷 저장 실패");
            return translated;
        }

        private static DateTime ParseDateTime(string value)
        {
            DateTime parsed;
            return DateTime.TryParse(value, out parsed) ? parsed : DateTime.MinValue;
        }

        private static string FormatTime(string occurredAt)
        {
            var parsed = ParseDateTime(occurredAt);
            return parsed == DateTime.MinValue ? occurredAt : parsed.ToString("HH:mm");
        }

        private static bool IsAll(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var trimmed = value.Trim();
            return trimmed.StartsWith("All", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("전체", StringComparison.OrdinalIgnoreCase);
        }
    }
}
