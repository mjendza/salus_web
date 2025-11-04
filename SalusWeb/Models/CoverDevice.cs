namespace SalusWeb.Models;

public class CoverDevice : DeviceBase
{
    public int? Position { get; set; }
    public string State { get; set; } = "closed";
    
    public CoverDevice()
    {
        DeviceType = "Cover";
    }
}
