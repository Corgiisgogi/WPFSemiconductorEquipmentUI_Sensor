using System;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public class ActivityLogItem
    {
        public long Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Time { get; set; }
        public string Source { get; set; }
        public string User { get; set; }
        public string Event { get; set; }
        public string Severity { get; set; }
        public string Saved { get; set; }

        public string DateText
        {
            get { return OccurredAt == DateTime.MinValue ? string.Empty : OccurredAt.ToString("yyyy-MM-dd"); }
        }
    }
}
