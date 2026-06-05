using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class UserSession : ViewModelBase
    {
        private string _userId;
        private string _roleCode;
        private string _approvalStatus;
        private string _roleText;
        private string _userStateText;
        private string _userStateTone;
        private bool _isApproved;
        private bool _isAdmin;

        public UserSession()
        {
            Logout();
        }

        public string UserId
        {
            get { return _userId; }
            private set { SetProperty(ref _userId, value); }
        }

        public string RoleCode
        {
            get { return _roleCode; }
            private set { SetProperty(ref _roleCode, value); }
        }

        public string ApprovalStatus
        {
            get { return _approvalStatus; }
            private set { SetProperty(ref _approvalStatus, value); }
        }

        public string RoleText
        {
            get { return _roleText; }
            private set { SetProperty(ref _roleText, value); }
        }

        public string UserStateText
        {
            get { return _userStateText; }
            private set { SetProperty(ref _userStateText, value); }
        }

        public string UserStateTone
        {
            get { return _userStateTone; }
            private set { SetProperty(ref _userStateTone, value); }
        }

        public bool IsApproved
        {
            get { return _isApproved; }
            private set { SetProperty(ref _isApproved, value); }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            private set { SetProperty(ref _isAdmin, value); }
        }

        public void ApplyAuthResult(AuthResult result)
        {
            if (result == null)
            {
                SetSession("guest", "SignedOut", "Locked");
                return;
            }

            var status = string.IsNullOrWhiteSpace(result.ApprovalStatus) ? (result.Success ? "Approved" : "Pending") : result.ApprovalStatus.Trim();
            var role = string.IsNullOrWhiteSpace(result.Role) ? "Operator" : result.Role.Trim();
            var userId = string.IsNullOrWhiteSpace(result.UserId) ? "guest" : result.UserId.Trim();
            SetSession(userId, role, status, result.Success && IsApprovedStatus(status));
        }

        public void ApplyRegisterResult(RegisterResult result)
        {
            if (result == null)
            {
                SetSession("guest", "SignedOut", "Locked");
                return;
            }

            var userId = string.IsNullOrWhiteSpace(result.UserId) ? "guest" : result.UserId.Trim();
            var status = string.IsNullOrWhiteSpace(result.ApprovalStatus) ? "Pending" : result.ApprovalStatus.Trim();
            SetSession(userId, "Pending", status, false);
        }

        public void LoginAsOperator(string userId)
        {
            SetSession(string.IsNullOrWhiteSpace(userId) ? "operator01" : userId.Trim(), "Operator", "Approved", true);
        }

        public void LoginAsAdmin()
        {
            SetSession("admin", "Admin", "Approved", true);
        }

        public void Logout()
        {
            SetSession("guest", "SignedOut", "Locked", false);
        }

        private void SetSession(string userId, string roleCode, string approvalStatus, bool approvedOverride = false)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? "guest" : userId;
            RoleCode = string.IsNullOrWhiteSpace(roleCode) ? "SignedOut" : roleCode;
            ApprovalStatus = string.IsNullOrWhiteSpace(approvalStatus) ? "Locked" : approvalStatus;
            RoleText = ToRoleText(RoleCode);
            UserStateText = ToStatusText(ApprovalStatus);
            UserStateTone = ToStatusTone(ApprovalStatus);
            IsApproved = approvedOverride && IsApprovedStatus(ApprovalStatus);
            IsAdmin = IsApproved && string.Equals(RoleCode, "Admin", System.StringComparison.OrdinalIgnoreCase);
            CommandManager.InvalidateRequerySuggested();
        }

        private static bool IsApprovedStatus(string status)
        {
            return string.Equals(status, "Approved", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string ToRoleText(string role)
        {
            if (string.Equals(role, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                return "관리자";
            }

            if (string.Equals(role, "Operator", System.StringComparison.OrdinalIgnoreCase))
            {
                return "작업자";
            }

            if (string.Equals(role, "Pending", System.StringComparison.OrdinalIgnoreCase))
            {
                return "승인 대기";
            }

            return "로그아웃";
        }

        private static string ToStatusText(string status)
        {
            if (string.Equals(status, "Approved", System.StringComparison.OrdinalIgnoreCase))
            {
                return "승인됨";
            }

            if (string.Equals(status, "Pending", System.StringComparison.OrdinalIgnoreCase))
            {
                return "승인 대기";
            }

            if (string.Equals(status, "Rejected", System.StringComparison.OrdinalIgnoreCase))
            {
                return "거절됨";
            }

            if (string.Equals(status, "Disabled", System.StringComparison.OrdinalIgnoreCase))
            {
                return "비활성";
            }

            return "잠김";
        }

        private static string ToStatusTone(string status)
        {
            if (string.Equals(status, "Approved", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Normal";
            }

            if (string.Equals(status, "Pending", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Warning";
            }

            return "Danger";
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
