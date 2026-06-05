using System;

namespace WPFSemiconductorEquipmentUI_Sensor.Models
{
    public sealed class UserAccount
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
