# Quick Start Guide - Salus iT600 Web Application

## What is this?

This is a C# web application that allows you to authenticate with your Salus iT600 smart home gateway using your EUID (Electronic Unique ID). Your credentials are securely stored in your browser session.

## Prerequisites

- .NET 9.0 SDK (or later) - [Download here](https://dotnet.microsoft.com/download)
- A Salus iT600 Gateway (UGE600 model)
- The EUID found on the bottom of your gateway (16 characters like `001E5E0D32906128`)

## Quick Start (5 minutes)

### Step 1: Run the Application

```bash
# Navigate to the application folder
cd SalusWeb

# Run the application
dotnet run
```

You should see output like:
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 2: Open in Browser

Open your web browser and go to:
```
http://localhost:5000
```

### Step 3: Login

You'll be automatically redirected to the login page:

1. **Gateway Host**: Enter your gateway's IP address (e.g., `192.168.1.100`)
2. **EUID**: Enter your 16-character EUID (e.g., `001E5E0D32906128`)
3. Click **Connect**

### Step 4: View Status

After successful login, you'll see:
- Your gateway host
- Your EUID
- When you authenticated
- Active session status

### Step 5: Logout

Click **Logout** in the navigation bar when done.

## Troubleshooting

### Can't connect with your EUID?

Try using `0000000000000000` instead of your actual EUID.

### Invalid EUID error?

Make sure your EUID is:
- Exactly 16 characters long
- Contains only numbers (0-9) and letters (A-F)

### Application won't start?

Make sure you have .NET 9.0 or later installed:
```bash
dotnet --version
```

### Port already in use?

Specify a different port:
```bash
dotnet run --urls="http://localhost:5001"
```

## Session Information

- **Duration**: Your session lasts 30 minutes of inactivity
- **Storage**: Credentials are stored server-side, not in cookies
- **Security**: Cookies are HTTP-only to prevent XSS attacks
- **Persistence**: Sessions are lost when the application restarts

## What's Next?

After authentication, the application currently only displays your connection information. This is a foundation that can be extended to:

- Control your heating/cooling devices
- Monitor sensor data
- View device status
- Configure device settings

See `CSHARP_IMPLEMENTATION.md` for technical details about extending the application.

## Need Help?

- Check the detailed README in `SalusWeb/README.md`
- Review the implementation guide in `CSHARP_IMPLEMENTATION.md`
- Look at the Python implementation in `custom_components/salus/` for reference

## Based On

This C# application is based on the Python Home Assistant integration for Salus iT600. The authentication mechanism mirrors the Python implementation while using ASP.NET Core for the web interface.
