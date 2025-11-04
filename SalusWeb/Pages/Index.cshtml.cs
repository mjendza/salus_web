using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SalusWeb.Models;
using SalusWeb.Services;

namespace SalusWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ISalusGatewayService _gatewayService;

    public IndexModel(ILogger<IndexModel> logger, ISalusGatewayService gatewayService)
    {
        _logger = logger;
        _gatewayService = gatewayService;
    }

    public string? Euid { get; set; }
    public string? Host { get; set; }
    public string? AuthenticatedAt { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<DeviceBase> Devices { get; set; } = new();
    public List<ClimateDevice> ClimateDevices { get; set; } = new();
    public List<SensorDevice> SensorDevices { get; set; } = new();
    public List<SwitchDevice> SwitchDevices { get; set; } = new();
    public List<BinarySensorDevice> BinarySensorDevices { get; set; } = new();
    public List<CoverDevice> CoverDevices { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is authenticated
        Euid = HttpContext.Session.GetString("EUID");
        Host = HttpContext.Session.GetString("Host");
        AuthenticatedAt = HttpContext.Session.GetString("AuthenticatedAt");
        
        IsAuthenticated = !string.IsNullOrEmpty(Euid);

        if (!IsAuthenticated)
        {
            return RedirectToPage("/Login");
        }

        // Fetch devices from gateway
        try
        {
            Devices = await _gatewayService.GetAllDevicesAsync(Host!, Euid!);
            ClimateDevices = Devices.OfType<ClimateDevice>().ToList();
            SensorDevices = Devices.OfType<SensorDevice>().ToList();
            SwitchDevices = Devices.OfType<SwitchDevice>().ToList();
            BinarySensorDevices = Devices.OfType<BinarySensorDevice>().ToList();
            CoverDevices = Devices.OfType<CoverDevice>().ToList();
            
            _logger.LogInformation("Successfully loaded {DeviceCount} devices", Devices.Count);
        }
        catch (SalusWeb.Exceptions.SalusAuthenticationException ex)
        {
            _logger.LogError(ex, "Authentication error loading devices from gateway");
            ErrorMessage = "Authentication failed. Please verify your EUID is correct (16 hexadecimal characters).";
        }
        catch (SalusWeb.Exceptions.SalusConnectionException ex)
        {
            _logger.LogError(ex, "Connection error loading devices from gateway");
            ErrorMessage = $"Cannot connect to gateway at {Host}. Please check:\n• Gateway is powered on\n• Host/IP address is correct\n• Gateway is on the same network\n• 'Local WiFi Mode' is enabled in gateway settings";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading devices from gateway");
            ErrorMessage = $"Error communicating with gateway: {ex.Message}";
        }

        return Page();
    }
}
