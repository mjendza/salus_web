using SalusWeb.Models;

namespace SalusWeb.Services;

public interface ISalusGatewayService
{
    Task<List<DeviceBase>> GetAllDevicesAsync(string host, string euid);
    Task<List<ClimateDevice>> GetClimateDevicesAsync(string host, string euid);
    Task<List<SensorDevice>> GetSensorDevicesAsync(string host, string euid);
    Task<List<SwitchDevice>> GetSwitchDevicesAsync(string host, string euid);
    Task<List<BinarySensorDevice>> GetBinarySensorDevicesAsync(string host, string euid);
    Task<List<CoverDevice>> GetCoverDevicesAsync(string host, string euid);
}
