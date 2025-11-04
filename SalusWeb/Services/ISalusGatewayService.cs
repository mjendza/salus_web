using SalusWeb.Models;

namespace SalusWeb.Services;

public interface ISalusGatewayService
{
    /// <summary>
    /// Tests connectivity to the gateway and validates authentication
    /// </summary>
    Task<bool> TestConnectionAsync(string host, string euid);
    
    Task<List<DeviceBase>> GetAllDevicesAsync(string host, string euid);
    Task<List<ClimateDevice>> GetClimateDevicesAsync(string host, string euid);
    Task<List<SensorDevice>> GetSensorDevicesAsync(string host, string euid);
    Task<List<SwitchDevice>> GetSwitchDevicesAsync(string host, string euid);
    Task<List<BinarySensorDevice>> GetBinarySensorDevicesAsync(string host, string euid);
    Task<List<CoverDevice>> GetCoverDevicesAsync(string host, string euid);
}
