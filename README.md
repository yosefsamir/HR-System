# HR System

A complete Human Resources Management System built with ASP.NET Core MVC.

## Features

- 👥 Employee Management
- 🏢 Department Management  
- 💰 Payroll Calculation & Management
- 📊 Attendance Tracking
- 🖨️ Salary Slip Printing (4 per A4 page)
- 🌙 Dark Theme with RTL Arabic Support

## Requirements

### For Development
- .NET 9.0 SDK
- SQL Server (any edition)
- Visual Studio / VS Code

### For Client Installation
- Windows 10/11
- SQL Server Express (free)
- No .NET Runtime needed (self-contained build)

## Installation (Client)

1. Download the latest release from [Releases](https://github.com/yosefsamir/HR-System/releases)
2. Extract the zip file
3. Run `Install.bat` as Administrator
4. Follow the installation wizard

## Development Setup

```bash
# Clone the repository
git clone https://github.com/yosefsamir/HR-System.git
cd HR-System

# Restore packages
dotnet restore

# Update connection string in appsettings.Development.json

# Run migrations
dotnet ef database update --project HR-system

# Run the application
dotnet run --project HR-system
```

## Building for Windows

```bash
cd Deployment
./build-for-windows.sh
```

This creates:
- `Deployment/publish/` - Ready to deploy folder
- `Deployment/HRSystem-v{version}.zip` - Release package

## Auto-Update

Clients can run `CheckForUpdates.bat` to automatically:
- Check GitHub for new versions
- Download and install updates
- Backup current version
- Restart the application

## Tech Stack

- **Framework:** ASP.NET Core 9.0 MVC
- **Database:** SQL Server with Entity Framework Core
- **Frontend:** Bootstrap 5, jQuery, Bootstrap Icons
- **Font:** Cairo (Arabic support)

## License

This project is proprietary software.

## Author

Yosef Samir - [@yosefsamir](https://github.com/yosefsamir)
