using System.Collections.ObjectModel;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class ConsoleViewModel : ScreenViewModelBase
    {
        public ConsoleViewModel()
        {
            Title = "WPF Equipment Control Console";
            Description = "Desktop interface for field control, sensor review, and activity monitoring.";

            Sensors = new ObservableCollection<SensorMetric>
            {
                new SensorMetric { Name = "Pressure", Value = "2.4", Unit = "bar", RangeText = "Normal 1.5 - 3.0 bar", BadgeText = "NORMAL", Tone = "Normal", IndicatorWidth = 145 },
                new SensorMetric { Name = "Distance", Value = "18", Unit = "cm", RangeText = "Normal 12 - 24 cm", BadgeText = "NORMAL", Tone = "Normal", IndicatorWidth = 145 },
                new SensorMetric { Name = "Temperature", Value = "38", Unit = "C", RangeText = "Watch over 40 C", BadgeText = "WATCH", Tone = "Warning", IndicatorWidth = 176 },
                new SensorMetric { Name = "Humidity", Value = "41", Unit = "%", RangeText = "Normal 30 - 60 %", BadgeText = "NORMAL", Tone = "Normal", IndicatorWidth = 145 }
            };

            ActivityLogs = SampleData.CreateLogs();
        }

        public ObservableCollection<SensorMetric> Sensors { get; private set; }
        public ObservableCollection<ActivityLogItem> ActivityLogs { get; private set; }
    }
}
