@echo off
title HR System - Check for Updates

echo ========================================
echo      HR System - Check for Updates
echo ========================================
echo.

cd /d "%~dp0"

REM Configuration - Your GitHub Repository
set GITHUB_USER=yosefsamir
set GITHUB_REPO=HR-System
set CURRENT_VERSION_FILE=version.txt

REM Read current version
if exist "%CURRENT_VERSION_FILE%" (
    set /p CURRENT_VERSION=<"%CURRENT_VERSION_FILE%"
) else (
    set CURRENT_VERSION=0.0.0
)

echo Current version: %CURRENT_VERSION%
echo.
echo Checking for updates...
echo.

REM Download latest version info from GitHub
set LATEST_URL=https://api.github.com/repos/%GITHUB_USER%/%GITHUB_REPO%/releases/latest

REM Use PowerShell to download and parse
powershell -Command "try { $release = Invoke-RestMethod -Uri '%LATEST_URL%'; $release.tag_name | Out-File -FilePath '%TEMP%\hr_latest_version.txt' -Encoding ASCII -NoNewline; $asset = $release.assets | Where-Object { $_.name -like '*.zip' } | Select-Object -First 1; if ($asset) { $asset.browser_download_url | Out-File -FilePath '%TEMP%\hr_download_url.txt' -Encoding ASCII -NoNewline }; Write-Host 'OK' } catch { Write-Host 'ERROR: ' + $_.Exception.Message; exit 1 }"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Could not connect to GitHub
    echo Please check your internet connection
    echo.
    pause
    exit /b 1
)

REM Read latest version
set /p LATEST_VERSION=<"%TEMP%\hr_latest_version.txt"
set /p DOWNLOAD_URL=<"%TEMP%\hr_download_url.txt"

REM Remove 'v' prefix if present for comparison
set LATEST_VERSION_CLEAN=%LATEST_VERSION:v=%

echo Latest version: %LATEST_VERSION%
echo.

REM Compare versions
if "%CURRENT_VERSION%"=="%LATEST_VERSION%" (
    echo ========================================
    echo    You have the latest version!
    echo ========================================
    echo.
    pause
    exit /b 0
)

if "%CURRENT_VERSION%"=="%LATEST_VERSION_CLEAN%" (
    echo ========================================
    echo    You have the latest version!
    echo ========================================
    echo.
    pause
    exit /b 0
)

echo ========================================
echo    New version available: %LATEST_VERSION%
echo ========================================
echo.
echo Do you want to download and install the update?
echo.
set /p CONFIRM="Type Y to update, N to cancel: "

if /i not "%CONFIRM%"=="Y" (
    echo Update cancelled.
    pause
    exit /b 0
)

echo.
echo Downloading update...
echo This may take a few minutes...
echo.

REM Download the update
set UPDATE_ZIP=%TEMP%\hr_update.zip
powershell -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri '%DOWNLOAD_URL%' -OutFile '%UPDATE_ZIP%'"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Download failed
    pause
    exit /b 1
)

echo Download complete!
echo.

REM Stop the running application
echo Stopping HR System...
taskkill /IM "HRSystem.exe" /F 2>nul
timeout /t 2 /nobreak >nul

REM Backup current version
echo Creating backup...
if not exist "Backup" mkdir Backup
set BACKUP_DIR=Backup\v%CURRENT_VERSION%_%date:~-4,4%%date:~-10,2%%date:~-7,2%
mkdir "%BACKUP_DIR%" 2>nul
copy "HRSystem.exe" "%BACKUP_DIR%\" >nul 2>&1
copy "version.txt" "%BACKUP_DIR%\" >nul 2>&1
echo    Backup saved to: %BACKUP_DIR%

REM Extract update
echo Extracting update...
if exist "Update" rmdir /S /Q "Update"
mkdir "Update"
powershell -Command "Expand-Archive -Path '%UPDATE_ZIP%' -DestinationPath 'Update' -Force"

REM Copy new files (but keep local config)
echo Installing update...
copy "appsettings.Production.json" "%TEMP%\appsettings.Production.json.bak" >nul 2>&1
xcopy /E /Y "Update\*" "." >nul
copy "%TEMP%\appsettings.Production.json.bak" "appsettings.Production.json" >nul 2>&1

REM Update version file
echo %LATEST_VERSION_CLEAN%> "%CURRENT_VERSION_FILE%"

REM Cleanup
del "%UPDATE_ZIP%" 2>nul
del "%TEMP%\appsettings.Production.json.bak" 2>nul
rmdir /S /Q "Update" 2>nul

echo.
echo ========================================
echo    Update completed successfully!
echo    New version: %LATEST_VERSION%
echo ========================================
echo.
echo Starting HR System...
timeout /t 2 >nul

REM Start the application
start "" "StartHRSystem.bat"

exit /b 0
