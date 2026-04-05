@echo off
title HR System - Start WhatsApp Service

cd /d "%~dp0"

set "AUTO_MODE=0"
if /I "%~1"=="--auto" set "AUTO_MODE=1"

if not exist "OpenWa\docker-compose.yml" (
    if "%AUTO_MODE%"=="1" (
        echo WARNING: OpenWa\docker-compose.yml not found. Skipping automatic WhatsApp startup.
        exit /b 0
    )
    echo ERROR: OpenWa\docker-compose.yml not found.
    echo Make sure OpenWa folder exists next to this file.
    pause
    exit /b 1
)

docker --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    if "%AUTO_MODE%"=="1" (
        echo WARNING: Docker is not installed or not available in PATH. Skipping automatic WhatsApp startup.
        exit /b 0
    )
    echo ERROR: Docker is not installed or not available in PATH.
    echo Install Docker Desktop, then run this file again.
    pause
    exit /b 1
)

echo Starting WhatsApp service...
docker compose -f "OpenWa\docker-compose.yml" --env-file "OpenWa\.env" down --remove-orphans >nul 2>&1

REM Cleanup stale Chromium profile locks that can remain after abrupt container stops.
docker compose -f "OpenWa\docker-compose.yml" --env-file "OpenWa\.env" run --rm --entrypoint sh openwa -c "find /app/data -type f \( -name 'SingletonLock' -o -name 'SingletonCookie' -o -name 'SingletonSocket' \) -delete" >nul 2>&1

docker compose -f "OpenWa\docker-compose.yml" --env-file "OpenWa\.env" up -d
if %ERRORLEVEL% NEQ 0 (
    if "%AUTO_MODE%"=="1" (
        echo WARNING: Failed to start WhatsApp service automatically.
        exit /b 0
    )
    echo ERROR: Failed to start WhatsApp service.
    pause
    exit /b 1
)

echo.
echo WhatsApp service is running.
echo API: http://localhost:2785/api
echo Dashboard: http://localhost:2886
echo.
echo Next step in HR System: Settings ^> WhatsApp, then scan QR.
if "%AUTO_MODE%"=="1" exit /b 0
pause
