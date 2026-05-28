using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    internal static class SampleData
    {
        public static ObservableCollection<ActivityLogItem> CreateLogs()
        {
            return new ObservableCollection<ActivityLogItem>
            {
                new ActivityLogItem { Time = "09:12:04", Source = "Auth", User = "operator01", Event = "Login OK", Severity = "INFO", Saved = "YES" },
                new ActivityLogItem { Time = "09:15:42", Source = "Control", User = "operator01", Event = "Motor Start command", Severity = "INFO", Saved = "YES" },
                new ActivityLogItem { Time = "09:18:11", Source = "AI Rule", User = "system", Event = "Pressure threshold approaching", Severity = "WARN", Saved = "YES" },
                new ActivityLogItem { Time = "09:20:35", Source = "Sensor", User = "system", Event = "Saved sensor snapshot", Severity = "INFO", Saved = "YES" },
                new ActivityLogItem { Time = "09:24:18", Source = "AI Rule", User = "system", Event = "AI watch condition armed", Severity = "WARN", Saved = "YES" },
                new ActivityLogItem { Time = "09:28:02", Source = "Admin", User = "admin", Event = "Remote shutdown received", Severity = "RISK", Saved = "YES" },
                new ActivityLogItem { Time = "09:28:04", Source = "Control", User = "system", Event = "Equipment stop completed", Severity = "INFO", Saved = "YES" },
                new ActivityLogItem { Time = "09:30:10", Source = "Storage", User = "system", Event = "SQLite audit trail flushed", Severity = "INFO", Saved = "YES" }
            };
        }
    }
}
