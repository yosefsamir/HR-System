@echo off
title Stop HR System

echo ========================================
echo         Stop HR System
echo ========================================
echo.
echo Stopping the system...
echo.

taskkill /IM "HRSystem.exe" /F 2>nul

if %ERRORLEVEL% EQU 0 (
    echo System stopped successfully.
) else (
    echo System is not running.
)

echo.
timeout /t 3 /nobreak >nul
