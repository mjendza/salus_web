using SalusWeb.Models;

namespace SalusWeb.Services;

public class SalusGatewayService : ISalusGatewayService
{
    private readonly ILogger<SalusGatewayService> _logger;

    public SalusGatewayService(ILogger<SalusGatewayService> logger)
    {
        _logger = logger;
    }

    public async Task<List<DeviceBase>> GetAllDevicesAsync(string host, string euid)
    {
        var allDevices = new List<DeviceBase>();
        
        try
        {
            // Try to get devices from the gateway
            // For now, we'll return mock data as a demonstration
            // In a real implementation, this would call the Salus gateway API
            
            allDevices.AddRange(await GetClimateDevicesAsync(host, euid));
            allDevices.AddRange(await GetSensorDevicesAsync(host, euid));
            allDevices.AddRange(await GetSwitchDevicesAsync(host, euid));
            allDevices.AddRange(await GetBinarySensorDevicesAsync(host, euid));
            allDevices.AddRange(await GetCoverDevicesAsync(host, euid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching devices from gateway {Host}", host);
            // Return mock data even if connection fails (for demonstration)
            allDevices = GetMockDevices();
        }

        return allDevices;
    }

    public Task<List<ClimateDevice>> GetClimateDevicesAsync(string host, string euid)
    {
        // In a real implementation, this would call the Salus gateway API
        // For now, return mock data for demonstration
        
        return Task.FromResult(new List<ClimateDevice>
        {
            new ClimateDevice
            {
                UniqueId = "thermostat_living_room",
                Name = "Living Room Thermostat",
                Model = "HTRP-RF",
                CurrentTemperature = 21.5,
                TargetTemperature = 22.0,
                HvacMode = "heat",
                PresetMode = "comfort",
                FanMode = "auto",
                Humidity = 45,
                MinTemperature = 5.0,
                MaxTemperature = 35.0,
                TemperatureUnit = "°C",
                Available = true
            },
            new ClimateDevice
            {
                UniqueId = "thermostat_bedroom",
                Name = "Bedroom Thermostat",
                Model = "HTRP-RF",
                CurrentTemperature = 19.8,
                TargetTemperature = 20.0,
                HvacMode = "heat",
                PresetMode = "sleep",
                FanMode = "low",
                Humidity = 48,
                MinTemperature = 5.0,
                MaxTemperature = 35.0,
                TemperatureUnit = "°C",
                Available = true
            }
        });
    }

    public Task<List<SensorDevice>> GetSensorDevicesAsync(string host, string euid)
    {
        return Task.FromResult(new List<SensorDevice>
        {
            new SensorDevice
            {
                UniqueId = "temp_sensor_kitchen",
                Name = "Kitchen Temperature",
                Model = "VS10RF",
                State = "22.3",
                Unit = "°C",
                DeviceClass = "temperature",
                Available = true
            },
            new SensorDevice
            {
                UniqueId = "humidity_sensor_bathroom",
                Name = "Bathroom Humidity",
                Model = "VS10RF",
                State = "65",
                Unit = "%",
                DeviceClass = "humidity",
                Available = true
            },
            new SensorDevice
            {
                UniqueId = "temp_sensor_outside",
                Name = "Outside Temperature",
                Model = "VS10RF",
                State = "12.5",
                Unit = "°C",
                DeviceClass = "temperature",
                Available = true
            }
        });
    }

    public Task<List<SwitchDevice>> GetSwitchDevicesAsync(string host, string euid)
    {
        return Task.FromResult(new List<SwitchDevice>
        {
            new SwitchDevice
            {
                UniqueId = "switch_boiler",
                Name = "Boiler Switch",
                Model = "SR600",
                IsOn = true,
                Available = true
            },
            new SwitchDevice
            {
                UniqueId = "switch_pump",
                Name = "Circulation Pump",
                Model = "SR600",
                IsOn = false,
                Available = true
            }
        });
    }

    public Task<List<BinarySensorDevice>> GetBinarySensorDevicesAsync(string host, string euid)
    {
        return Task.FromResult(new List<BinarySensorDevice>
        {
            new BinarySensorDevice
            {
                UniqueId = "motion_hallway",
                Name = "Hallway Motion",
                Model = "PS600",
                IsOn = false,
                DeviceClass = "motion",
                Available = true
            },
            new BinarySensorDevice
            {
                UniqueId = "door_front",
                Name = "Front Door",
                Model = "OS600",
                IsOn = false,
                DeviceClass = "door",
                Available = true
            }
        });
    }

    public Task<List<CoverDevice>> GetCoverDevicesAsync(string host, string euid)
    {
        return Task.FromResult(new List<CoverDevice>
        {
            new CoverDevice
            {
                UniqueId = "blind_living_room",
                Name = "Living Room Blind",
                Model = "RS600",
                Position = 50,
                State = "open",
                Available = true
            }
        });
    }

    private List<DeviceBase> GetMockDevices()
    {
        var devices = new List<DeviceBase>();
        
        devices.Add(new ClimateDevice
        {
            UniqueId = "thermostat_demo",
            Name = "Demo Thermostat",
            Model = "HTRP-RF",
            CurrentTemperature = 20.0,
            TargetTemperature = 21.0,
            HvacMode = "heat",
            Available = true
        });
        
        devices.Add(new SensorDevice
        {
            UniqueId = "sensor_demo",
            Name = "Demo Temperature Sensor",
            Model = "VS10RF",
            State = "20.5",
            Unit = "°C",
            DeviceClass = "temperature",
            Available = true
        });
        
        return devices;
    }
}
