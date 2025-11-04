using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SalusWeb.Services;
using SalusWeb.Exceptions;

namespace SalusWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly ISalusGatewayService _gatewayService;

        public LoginModel(ILogger<LoginModel> logger, ISalusGatewayService gatewayService)
        {
            _logger = logger;
            _gatewayService = gatewayService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Gateway host is required")]
        public string Host { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "EUID is required")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "EUID must be exactly 16 characters")]
        [RegularExpression("^[0-9A-Fa-f]{16}$", ErrorMessage = "EUID must be 16 hexadecimal characters")]
        public string Euid { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Check if already authenticated
            var existingEuid = HttpContext.Session.GetString("EUID");
            if (!string.IsNullOrEmpty(existingEuid))
            {
                Response.Redirect("/");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Validate EUID format
                if (Euid.Length != 16)
                {
                    ErrorMessage = "EUID must be exactly 16 characters";
                    return Page();
                }

                // Test connection to the gateway before storing credentials
                _logger.LogInformation("Testing connection to gateway at {Host}", Host);
                
                await _gatewayService.TestConnectionAsync(Host, Euid);

                // Connection successful - store authentication information in session
                HttpContext.Session.SetString("EUID", Euid);
                HttpContext.Session.SetString("Host", Host);
                HttpContext.Session.SetString("AuthenticatedAt", DateTime.UtcNow.ToString("o"));

                _logger.LogInformation("User authenticated with EUID: {Euid} and Host: {Host}", 
                    Euid.Substring(0, 4) + "..." + Euid.Substring(12), Host);

                // Redirect to home page
                return RedirectToPage("/Index");
            }
            catch (SalusAuthenticationException ex)
            {
                _logger.LogWarning(ex, "Authentication failed");
                ErrorMessage = "Authentication failed. Please verify your EUID is correct (16 hexadecimal characters found on the bottom of your gateway).";
                return Page();
            }
            catch (SalusConnectionException ex)
            {
                _logger.LogWarning(ex, "Connection failed");
                ErrorMessage = $"Cannot connect to gateway at {Host}. Please check:\n• Gateway is powered on\n• Host/IP address is correct\n• Gateway is on the same network\n• 'Local WiFi Mode' is enabled in gateway settings";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                return Page();
            }
        }
    }
}
