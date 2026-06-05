namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public sealed class AuthResult
    {
        public bool Success { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public string ApprovalStatus { get; set; }
        public string Message { get; set; }
    }
}
