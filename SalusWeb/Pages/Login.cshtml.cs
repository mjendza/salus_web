using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SalusWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
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

        public IActionResult OnPost()
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

                // Store authentication information in session
                HttpContext.Session.SetString("EUID", Euid);
                HttpContext.Session.SetString("Host", Host);
                HttpContext.Session.SetString("AuthenticatedAt", DateTime.UtcNow.ToString("o"));

                _logger.LogInformation("User authenticated with EUID: {Euid} and Host: {Host}", 
                    Euid.Substring(0, 4) + "..." + Euid.Substring(12), Host);

                // Redirect to home page
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                ErrorMessage = "An error occurred during authentication. Please try again.";
                return Page();
            }
        }
    }
}
