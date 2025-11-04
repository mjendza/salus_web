namespace SalusWeb.Models;

public class SwitchDevice : DeviceBase
{
    public bool IsOn { get; set; }
    
    public SwitchDevice()
    {
        DeviceType = "Switch";
    }
}
