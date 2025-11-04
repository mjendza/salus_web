namespace SalusWeb.Models;

public class SensorDevice : DeviceBase
{
    public string? State { get; set; }
    public string? Unit { get; set; }
    public string? DeviceClass { get; set; }
    
    public SensorDevice()
    {
        DeviceType = "Sensor";
    }
}
