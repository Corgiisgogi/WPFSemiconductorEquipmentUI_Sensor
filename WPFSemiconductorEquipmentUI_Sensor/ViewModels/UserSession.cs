using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class UserSession : ViewModelBase
    {
        private string _userId;
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
                SetBlocked("guest", "Signed out", "LOCKED", "Danger");
                return;
            }

            var status = string.IsNullOrWhiteSpace(result.ApprovalStatus) ? (result.Success ? "Approved" : "Pending") : result.ApprovalStatus.Trim();
            var role = string.IsNullOrWhiteSpace(result.Role) ? "Operator" : result.Role.Trim();
            var userId = string.IsNullOrWhiteSpace(result.UserId) ? "guest" : result.UserId.Trim();

            if (IsApprovedStatus(status) && result.Success)
            {
                UserId = userId;
                RoleText = role;
                UserStateText = "APPROVED";
                UserStateTone = "Normal";
                IsApproved = true;
                IsAdmin = string.Equals(role, "Admin", System.StringComparison.OrdinalIgnoreCase);
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            SetBlocked(userId, role, status.ToUpperInvariant(), StatusTone(status));
        }

        public void ApplyRegisterResult(RegisterResult result)
        {
            if (result == null)
            {
                SetBlocked("guest", "Signed out", "LOCKED", "Danger");
                return;
            }

            var userId = string.IsNullOrWhiteSpace(result.UserId) ? "guest" : result.UserId.Trim();
            var status = string.IsNullOrWhiteSpace(result.ApprovalStatus) ? "Pending" : result.ApprovalStatus.Trim();
            SetBlocked(userId, "Pending approval", status.ToUpperInvariant(), StatusTone(status));
        }

        public void LoginAsOperator(string userId)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? "operator01" : userId.Trim();
            RoleText = "Operator";
            UserStateText = "APPROVED";
            UserStateTone = "Normal";
            IsApproved = true;
            IsAdmin = false;
            CommandManager.InvalidateRequerySuggested();
        }

        public void LoginAsAdmin()
        {
            UserId = "admin";
            RoleText = "Admin";
            UserStateText = "APPROVED";
            UserStateTone = "Normal";
            IsApproved = true;
            IsAdmin = true;
            CommandManager.InvalidateRequerySuggested();
        }

        public void Logout()
        {
            UserId = "guest";
            RoleText = "Signed out";
            UserStateText = "LOCKED";
            UserStateTone = "Danger";
            IsApproved = false;
            IsAdmin = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void SetBlocked(string userId, string role, string state, string tone)
        {
            UserId = string.IsNullOrWhiteSpace(userId) ? "guest" : userId;
            RoleText = string.IsNullOrWhiteSpace(role) ? "Signed out" : role;
            UserStateText = string.IsNullOrWhiteSpace(state) ? "LOCKED" : state;
            UserStateTone = string.IsNullOrWhiteSpace(tone) ? "Danger" : tone;
            IsApproved = false;
            IsAdmin = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private static bool IsApprovedStatus(string status)
        {
            return string.Equals(status, "Approved", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string StatusTone(string status)
        {
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
