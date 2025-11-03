namespace SalusWeb.Models;

public class BinarySensorDevice : DeviceBase
{
    public bool IsOn { get; set; }
    public string? DeviceClass { get; set; }
    
    public BinarySensorDevice()
    {
        DeviceType = "Binary Sensor";
    }
}
