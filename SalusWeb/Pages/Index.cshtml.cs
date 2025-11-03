using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SalusWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public string? Euid { get; set; }
    public string? Host { get; set; }
    public string? AuthenticatedAt { get; set; }
    public bool IsAuthenticated { get; set; }

    public IActionResult OnGet()
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

        return Page();
    }
}
