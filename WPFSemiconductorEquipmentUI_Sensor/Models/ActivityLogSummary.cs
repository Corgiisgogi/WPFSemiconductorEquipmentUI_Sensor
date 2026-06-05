namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public sealed class ActivityLogSummary
    {
        public int TotalCount { get; set; }
        public int WarningCount { get; set; }
        public int RiskCount { get; set; }
        public string LastShutdownTimeText { get; set; }
    }
}
