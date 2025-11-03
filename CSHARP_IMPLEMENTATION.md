# C# Web Application Implementation

## Overview

This document describes the C# ASP.NET Core Razor Pages web application implementation for Salus iT600 Gateway authentication, based on the Python Home Assistant integration.

## Implementation Details

### Location
The C# application is located in the `SalusWeb/` directory at the root of the repository.

### Technology Stack
- **Framework**: ASP.NET Core 9.0
- **UI**: Razor Pages
- **Session Management**: ASP.NET Core Session Middleware
- **Frontend**: Bootstrap 5 (included via scaffolding)

### Architecture

The application follows the Razor Pages architecture pattern:

```
SalusWeb/
├── Pages/                      # Razor Pages
│   ├── Login.cshtml           # Login page UI
│   ├── Login.cshtml.cs        # Login page logic
│   ├── Logout.cshtml          # Logout page UI
│   ├── Logout.cshtml.cs       # Logout page logic
│   ├── Index.cshtml           # Home page UI (protected)
│   ├── Index.cshtml.cs        # Home page logic
│   └── Shared/
│       └── _Layout.cshtml     # Main layout with navigation
├── Program.cs                  # App configuration & startup
└── wwwroot/                   # Static files (CSS, JS, libraries)
```

## Key Components

### 1. Session Configuration (Program.cs)

```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

- Sessions stored in memory
- 30-minute idle timeout
- HTTP-only cookies for security
- Essential cookies (exempt from cookie consent requirements)

### 2. Authentication Logic (Login.cshtml.cs)

**EUID Validation:**
- Must be exactly 16 characters
- Must contain only hexadecimal characters (0-9, A-F)
- Validated using Regular Expression: `^[0-9A-Fa-f]{16}$`

**Session Storage:**
- `EUID`: The 16-character Electronic Unique ID
- `Host`: Gateway IP address or hostname
- `AuthenticatedAt`: ISO 8601 timestamp

### 3. Protected Pages

The `Index.cshtml.cs` implements authentication check:

```csharp
public IActionResult OnGet()
{
    Euid = HttpContext.Session.GetString("EUID");
    // ... check other session values
    
    if (!IsAuthenticated)
    {
        return RedirectToPage("/Login");
    }
    
    return Page();
}
```

### 4. Logout Functionality (Logout.cshtml.cs)

```csharp
HttpContext.Session.Clear();
```

Clears all session data, effectively logging out the user.

## Comparison with Python Implementation

### Similarities

| Feature | Python (Home Assistant) | C# (Web App) |
|---------|------------------------|--------------|
| EUID Validation | 16 characters, validated in config_flow.py | 16 characters, validated with regex |
| Host Configuration | Stored in config entry | Stored in session |
| Authentication Storage | Home Assistant config entries | Browser session |
| EUID Format | `CONF_TOKEN` constant | Session key "EUID" |

### Key Differences

| Aspect | Python | C# |
|--------|--------|-----|
| **Storage** | Persistent (config entries) | Session-based (memory) |
| **Lifetime** | Permanent until removed | 30 minutes idle timeout |
| **Integration** | Home Assistant platform | Standalone web app |
| **Gateway Connection** | Uses pyit600 library | No actual connection (auth only) |

## Security Features

1. **Session Security**
   - HTTP-only cookies prevent XSS access
   - Server-side session storage
   - No sensitive data in cookies

2. **Input Validation**
   - EUID format validation
   - Required field validation
   - Model binding validation

3. **Automatic Protection**
   - Unauthenticated users redirected to login
   - Session timeout after inactivity
   - HTTPS enforced in production

## Running the Application

### Development
```bash
cd SalusWeb
dotnet run
```

Access at: http://localhost:5000

### Production Build
```bash
cd SalusWeb
dotnet publish -c Release -o ./publish
```

## Testing the Implementation

The application has been tested with the following scenarios:

1. ✅ **Login with valid EUID** - Successfully authenticates and stores session
2. ✅ **Session persistence** - Session data maintained across page navigation
3. ✅ **Protected page access** - Unauthenticated users redirected to login
4. ✅ **Navigation bar updates** - Shows connection status when authenticated
5. ✅ **Logout functionality** - Clears session and redirects to logout page
6. ✅ **Re-authentication required** - After logout, must login again to access protected pages

## Future Enhancements

Potential improvements that could be added:

1. **Actual Gateway Connection**: Integrate with Salus Gateway API
2. **Persistent Sessions**: Use distributed cache (Redis) for multi-instance deployments
3. **Device Discovery**: Auto-detect gateways on the local network
4. **Device Control**: Add pages to control thermostats, sensors, etc.
5. **Connection Health**: Monitor and display gateway connection status
6. **User Management**: Support multiple user accounts
7. **Settings Page**: Configure session timeout and other preferences

## Notes

- This implementation focuses on the authentication mechanism only
- No actual connection to the Salus Gateway is made (would require pyit600 equivalent in .NET)
- Session storage is in-memory; data is lost on application restart
- For production use, consider implementing persistent session storage

## References

- Python Implementation: `custom_components/salus/`
- pyit600 Library: https://github.com/epoplavskis/pyit600
- ASP.NET Core Session: https://learn.microsoft.com/aspnet/core/fundamentals/app-state
