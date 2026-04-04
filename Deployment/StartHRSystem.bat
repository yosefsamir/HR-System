@echo off
title HR System

cd /d "%~dp0"

REM Check if the app exists
if not exist "HRSystem.exe" (
    echo ERROR: Application file not found
    echo Please run Install.bat first
    pause
    exit /b 1
)

echo Starting HR System...
echo.

REM Start WhatsApp service automatically in background (if Docker/OpenWa are available)
if exist "StartWhatsAppService.bat" (
    start "" /min cmd /c ""%~dp0StartWhatsAppService.bat" --auto"
)

set LOCAL_IP=
for /f "tokens=14" %%i in ('ipconfig ^| findstr /R /C:"IPv4 Address" /C:"IPv4 العنوان"') do (
    if not defined LOCAL_IP set LOCAL_IP=%%i
)

if defined LOCAL_IP (
    set LOCAL_IP=%LOCAL_IP: =%
)

REM Open browser
start "" "http://localhost:5000"

REM Wait a moment
timeout /t 2 >nul

REM Run the application hidden (minimized)
start /min "" HRSystem.exe

echo HR System is running in background.
echo.
echo Local URL: http://localhost:5000
if defined LOCAL_IP echo Network URL: http://%LOCAL_IP%:5000
echo Use the Network URL from other devices on the same Wi-Fi/LAN.
echo.
echo To stop the system, run StopHRSystem.bat
echo.
timeout /t 3 >nul
