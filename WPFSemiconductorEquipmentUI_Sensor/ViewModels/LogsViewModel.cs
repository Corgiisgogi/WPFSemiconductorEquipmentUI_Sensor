using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;
using WPFSemiconductorEquipmentUI_Sensor.Services;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LogsViewModel : ScreenViewModelBase
    {
        private ActivityLogItem _selectedLog;

        public LogsViewModel()
            : this(new ActivityLogStore())
        {
        }

        public LogsViewModel(ActivityLogStore activityLogStore)
        {
            Title = "Activity & Sensor Logs";
            Description = "Filter and inspect activity logs and sensor records stored in SQLite.";

            ActivityLogs = activityLogStore.Logs;
            if (ActivityLogs.Count > 0)
            {
                SelectedLog = ActivityLogs.Count > 5 ? ActivityLogs[5] : ActivityLogs[0];
            }
        }

        public ObservableCollection<ActivityLogItem> ActivityLogs { get; private set; }

        public ActivityLogItem SelectedLog
        {
            get { return _selectedLog; }
            set
            {
                if (object.Equals(_selectedLog, value))
                {
                    return;
                }

                _selectedLog = value;
                OnPropertyChanged();
            }
        }
    }
}
