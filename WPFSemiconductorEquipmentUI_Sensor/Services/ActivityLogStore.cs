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
        private readonly IRemoteTelemetryService _remoteTelemetryService;

        public ActivityLogStore()
            : this(null, null)
        {
        }

        public ActivityLogStore(ActivityLogRepository activityLogRepository)
            : this(activityLogRepository, null)
        {
        }

        public ActivityLogStore(ActivityLogRepository activityLogRepository, IRemoteTelemetryService remoteTelemetryService)
        {
            _activityLogRepository = activityLogRepository;
            _remoteTelemetryService = remoteTelemetryService;
            Logs = LoadInitialLogs();
        }

        public ObservableCollection<ActivityLogItem> Logs { get; private set; }

        public void Add(string source, string user, string eventText, string severity)
        {
            var now = DateTime.Now;
            var log = new ActivityLogItem
            {
                OccurredAt = now,
                Time = now.ToString("HH:mm:ss"),
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
                    log.Id = _activityLogRepository.Insert(log);
                    log.Saved = "YES";
                }
                catch
                {
                    log.Saved = "NO";
                }
            }

            Logs.Insert(0, log);
            if (_remoteTelemetryService != null)
            {
                _remoteTelemetryService.SendActivityLog(log);
            }
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
