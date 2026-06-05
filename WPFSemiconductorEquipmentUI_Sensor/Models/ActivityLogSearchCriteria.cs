using System;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public sealed class ActivityLogSearchCriteria
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string User { get; set; }
        public string Source { get; set; }
        public string Severity { get; set; }
        public string Keyword { get; set; }
        public int Limit { get; set; }
    }
}
