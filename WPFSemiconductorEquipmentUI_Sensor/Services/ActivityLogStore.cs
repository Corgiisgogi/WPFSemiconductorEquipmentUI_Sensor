using System;
using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.ViewModels;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public sealed class ActivityLogStore
    {
        public ActivityLogStore()
        {
            Logs = SampleData.CreateLogs();
        }

        public ObservableCollection<ActivityLogItem> Logs { get; private set; }

        public void Add(string source, string user, string eventText, string severity)
        {
            Logs.Insert(0, new ActivityLogItem
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Source = source,
                User = string.IsNullOrWhiteSpace(user) ? "system" : user,
                Event = eventText,
                Severity = severity,
                Saved = "NO"
            });
        }
    }
}
