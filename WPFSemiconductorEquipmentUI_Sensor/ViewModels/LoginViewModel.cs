namespace WPFSemiconductorEquipmentUI_Sensor.ViewModels
{
    public class LoginViewModel : ScreenViewModelBase
    {
        public LoginViewModel()
        {
            Title = "Login / Sign up";
            Description = "Only approved operators can enter the equipment control console.";
            UserId = "operator01";
            Department = "Process Equipment";
        }

        public string UserId { get; set; }
        public string Department { get; set; }
    }
}
