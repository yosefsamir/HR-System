@echo off
title HR System - Stop WhatsApp Service

cd /d "%~dp0"

if not exist "OpenWa\docker-compose.yml" (
    echo ERROR: OpenWa\docker-compose.yml not found.
    pause
    exit /b 1
)

echo Stopping WhatsApp service...
docker compose -f "OpenWa\docker-compose.yml" --env-file "OpenWa\.env" down
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to stop WhatsApp service.
    pause
    exit /b 1
)

echo WhatsApp service stopped.
pause
