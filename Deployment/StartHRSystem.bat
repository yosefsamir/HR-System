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

REM Open browser
start "" "http://localhost:5009"

REM Wait a moment
timeout /t 2 >nul

REM Run the application hidden (minimized)
start /min "" HRSystem.exe

echo HR System is running in background.
echo.
echo To stop the system, run StopHRSystem.bat
echo.
timeout /t 3 >nul
