using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public interface IAuthService
    {
        AuthResult Login(string userId, string password);
        RegisterResult Register(string userId, string password, string displayName);
        AuthResult GetStatus(string userId);
    }
}
