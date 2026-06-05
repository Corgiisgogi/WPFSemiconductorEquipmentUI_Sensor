using WPFSemiconductorEquipmentUI_Sensor.Models;

namespace WPFSemiconductorEquipmentUI_Sensor.Services
{
    public interface IRemoteTelemetryService
    {
        void SendSensorSnapshot(SensorSnapshotRecord snapshot);
        void SendActivityLog(ActivityLogItem log);
    }
}
