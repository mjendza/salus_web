using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SalusWeb.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var euid = HttpContext.Session.GetString("EUID");
            if (!string.IsNullOrEmpty(euid))
            {
                _logger.LogInformation("User logged out with EUID: {Euid}", 
                    euid.Substring(0, 4) + "..." + euid.Substring(12));
            }

            // Clear session
            HttpContext.Session.Clear();

            return Page();
        }
    }
}
