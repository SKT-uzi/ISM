# ISM Demo

ISM Demo is a local demonstration and debugging toolkit that includes a web-based management interface and a Windows MQTT simulator.

## Project Structure

- `ISMDemo/`: An ASP.NET Core 8 web application that provides the management interface and an MQTT-over-WebSocket service.
- `ISMSimulator/`: A .NET Framework 4.6.2 Windows Forms application for connecting to the MQTT service and sending test messages.

## Requirements

- Windows 10 or Windows 11
- Visual Studio 2022 with the ASP.NET, desktop development, and .NET Framework 4.6.2 targeting-pack workloads
- .NET 8 SDK

## Run the Web Application

From the repository root, run:

```powershell
dotnet restore .\ISMDemo\ISMDemo.csproj
dotnet run --project .\ISMDemo\ISMDemo.csproj --launch-profile ChuteSideISMWebApp
```

The default URL is `https://localhost:8101/ism`.

Local launch settings are defined in `ISMDemo/Properties/launchSettings.json`. The MQTT credentials and encryption key in that file are development-only examples and must be replaced through a secure configuration mechanism before deployment.

## Run the Simulator

1. Open `ISMSimulator/ISMSimulator.sln` in Visual Studio.
2. Restore the NuGet packages.
3. Build and run `ISMSimulator`.
4. Connect to `wss://localhost:8101/ism/mqtt`. The simulator's development credentials match the web application's launch profile.

If the simulator requires local IoT Central settings, copy the example file:

```powershell
Copy-Item .\ISMSimulator\AppSettings.local.config.example .\ISMSimulator\AppSettings.local.config
```

Edit only `AppSettings.local.config`. Git ignores this file, so access tokens and other credentials remain local.

## Security

- Never commit production passwords, tokens, certificates, or connection strings.
- The sample configuration is intended for local development only and must not be used in production.
- Rotate any credential that has previously been stored in an unprotected location.
