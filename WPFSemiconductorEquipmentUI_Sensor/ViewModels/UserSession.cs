using System.Runtime.CompilerServices;
using System.Windows.Input;

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
            LoginAsOperator("operator01");
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
