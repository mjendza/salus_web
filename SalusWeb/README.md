# Salus iT600 Web Application

A simple C# ASP.NET Core Razor Pages web application for authenticating with the Salus iT600 Gateway using EUID (Electronic Unique ID) and storing credentials in browser sessions.

## Overview

This application provides a web-based interface to authenticate with Salus iT600 smart home gateways. It was created based on the Python Home Assistant implementation found in the `custom_components/salus` directory.

## Features

- **EUID Authentication**: Login using your 16-character Electronic Unique ID
- **Session Management**: Credentials are securely stored in browser sessions
- **Automatic Redirection**: Unauthenticated users are redirected to the login page
- **Connection Status**: View your current connection details and session information
- **Simple Logout**: Clear your session and disconnect from the gateway

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- A Salus iT600 Gateway (UGE600 model)

### Running the Application

1. Navigate to the application directory:
   ```bash
   cd SalusWeb
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   ```
   https://localhost:5001
   ```
   or
   ```
   http://localhost:5000
   ```

### Using the Application

1. **Login**: 
   - Enter your Gateway's IP address or hostname
   - Enter your 16-character EUID (found on the bottom of your gateway)
   - Click "Connect"

2. **View Status**:
   - After successful login, you'll see your connection details
   - The navigation bar shows your connected EUID
   - Session information is displayed including authentication time

3. **Logout**:
   - Click "Logout" in the navigation bar or on the home page
   - Your session will be cleared

## Configuration

The application uses ASP.NET Core's built-in session management:
- Session timeout: 30 minutes of inactivity
- Session data is stored in memory (not persistent across restarts)
- Cookies are HTTP-only and marked as essential

## Session Storage

The following information is stored in the browser session:
- `EUID`: The 16-character Electronic Unique ID
- `Host`: The gateway's IP address or hostname
- `AuthenticatedAt`: Timestamp when authentication occurred

## Troubleshooting

- **Can't connect with your EUID?** Try using `0000000000000000` instead
- **Connection issues?** Make sure "Local WiFi Mode" is enabled in the Smart Home app
- **Still not working?** Restart your gateway by unplugging and plugging back the USB power

## Project Structure

```
SalusWeb/
├── Pages/
│   ├── Index.cshtml          # Home page (requires authentication)
│   ├── Index.cshtml.cs
│   ├── Login.cshtml          # Login page
│   ├── Login.cshtml.cs
│   ├── Logout.cshtml         # Logout page
│   ├── Logout.cshtml.cs
│   ├── Privacy.cshtml
│   ├── Privacy.cshtml.cs
│   └── Shared/
│       └── _Layout.cshtml    # Main layout with navigation
├── Program.cs                # Application startup and configuration
├── appsettings.json
└── README.md
```

## Based On

This implementation is based on the Python Home Assistant integration for Salus iT600, which can be found in the `custom_components/salus` directory. Key concepts translated from Python to C#:

- EUID validation (16 hexadecimal characters)
- Host configuration for gateway connection
- Session-based authentication storage

## Security Notes

- EUID and credentials are stored in server-side sessions, not in cookies
- Sessions use HTTP-only cookies to prevent XSS attacks
- Sessions expire after 30 minutes of inactivity
- No passwords are stored; only the EUID is used for authentication

## License

This project follows the same license as the parent repository.
