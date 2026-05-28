using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LogsViewModel : ScreenViewModelBase
    {
        public LogsViewModel()
        {
            Title = "Activity & Sensor Logs";
            Description = "Filter and inspect activity logs and sensor records stored in SQLite.";

            ActivityLogs = SampleData.CreateLogs();
            SelectedLog = ActivityLogs[5];
        }

        public ObservableCollection<ActivityLogItem> ActivityLogs { get; private set; }
        public ActivityLogItem SelectedLog { get; set; }
    }
}
