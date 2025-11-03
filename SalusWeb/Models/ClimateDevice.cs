namespace SalusWeb.Models;

public class ClimateDevice : DeviceBase
{
    public double? CurrentTemperature { get; set; }
    public double? TargetTemperature { get; set; }
    public string HvacMode { get; set; } = "off";
    public string PresetMode { get; set; } = "none";
    public string FanMode { get; set; } = "auto";
    public int? Humidity { get; set; }
    public double? MinTemperature { get; set; }
    public double? MaxTemperature { get; set; }
    public string TemperatureUnit { get; set; } = "°C";
    
    public ClimateDevice()
    {
        DeviceType = "Climate";
    }
}
