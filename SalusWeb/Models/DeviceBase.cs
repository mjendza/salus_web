namespace SalusWeb.Models;

public class DeviceBase
{
    public string UniqueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = "Salus";
    public string Model { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public bool Available { get; set; } = true;
}
