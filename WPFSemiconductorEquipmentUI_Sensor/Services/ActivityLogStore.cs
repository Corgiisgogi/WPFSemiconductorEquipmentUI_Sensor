using System;
using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.ViewModels;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class ActivityLogStore
    {
        private const int RecentLogLimit = 300;
        private readonly ActivityLogRepository _activityLogRepository;

        public ActivityLogStore()
            : this(null)
        {
        }

        public ActivityLogStore(ActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
            Logs = LoadInitialLogs();
        }

        public ObservableCollection<ActivityLogItem> Logs { get; private set; }

        public void Add(string source, string user, string eventText, string severity)
        {
            var log = new ActivityLogItem
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Source = source,
                User = string.IsNullOrWhiteSpace(user) ? "system" : user,
                Event = eventText,
                Severity = severity,
                Saved = "NO"
            };

            if (_activityLogRepository != null)
            {
                try
                {
                    _activityLogRepository.Insert(log);
                    log.Saved = "YES";
                }
                catch
                {
                    log.Saved = "NO";
                }
            }

            Logs.Insert(0, log);
        }

        private ObservableCollection<ActivityLogItem> LoadInitialLogs()
        {
            if (_activityLogRepository != null)
            {
                try
                {
                    var logs = _activityLogRepository.LoadRecent(RecentLogLimit);
                    if (logs.Count > 0)
                    {
                        return logs;
                    }
                }
                catch
                {
                }
            }

            return SampleData.CreateLogs();
        }
    }
}
