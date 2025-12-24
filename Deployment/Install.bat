@echo off
title HR System Installation

echo ========================================
echo        HR System Installation
echo ========================================
echo.
echo This version includes all requirements
echo Only SQL Server is needed
echo.

cd /d "%~dp0"

REM Check for admin rights
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ========================================
    echo ERROR: Please run this file as Administrator
    echo ========================================
    echo.
    echo Right-click on this file and choose
    echo "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo [1/4] Checking SQL Server...

set SQL_INSTANCE=

REM Check for default SQL Server instance
sc query "MSSQLSERVER" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    set SQL_INSTANCE=localhost
    echo    OK: SQL Server found
    goto :sql_found
)

REM Check for SQL Express instance
sc query "MSSQL$SQLEXPRESS" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    set SQL_INSTANCE=localhost\SQLEXPRESS
    echo    OK: SQL Server Express found
    goto :sql_found
)

echo.
echo ========================================
echo    ERROR: SQL Server not found!
echo ========================================
echo.
echo Please install SQL Server Express first:
echo.
echo 1. Open browser and go to:
echo    https://www.microsoft.com/sql-server/sql-server-downloads
echo.
echo 2. Scroll down, choose "Express", then "Download now"
echo.
echo 3. Run the downloaded file and choose "Basic"
echo.
echo 4. After installation, restart the computer
echo.
echo 5. Run this file again
echo.
start "" "https://www.microsoft.com/en-us/sql-server/sql-server-downloads"
pause
exit /b 1

:sql_found

REM Create installation directory
echo.
echo [2/4] Creating installation folder...
set INSTALL_DIR=C:\HRSystem
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%\Backup" mkdir "%INSTALL_DIR%\Backup"
if not exist "%INSTALL_DIR%\Update" mkdir "%INSTALL_DIR%\Update"
if not exist "%INSTALL_DIR%\Logs" mkdir "%INSTALL_DIR%\Logs"
echo    OK: C:\HRSystem created

REM Copy application files
echo.
echo [3/4] Copying application files...
xcopy /E /Y /Q "." "%INSTALL_DIR%\" >nul 2>&1
echo    OK: Files copied

REM Update connection string with detected SQL instance
echo.
echo [4/4] Configuring database connection...
set CONFIG_FILE=%INSTALL_DIR%\appsettings.Production.json

REM Create the config file with the correct SQL instance
(
echo {
echo   "ConnectionStrings": {
echo     "DefaultConnection": "Server=%SQL_INSTANCE%;Database=HRSystemDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
echo   },
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Warning",
echo       "Microsoft.AspNetCore": "Warning"
echo     }
echo   },
echo   "AllowedHosts": "*",
echo   "Urls": "http://localhost:5009"
echo }
) > "%CONFIG_FILE%"
echo    OK: Connected to %SQL_INSTANCE%

REM Create desktop shortcut
echo.
echo Creating desktop shortcut...
set SHORTCUT_PATH=%USERPROFILE%\Desktop\HR System.lnk

echo Set oWS = WScript.CreateObject("WScript.Shell") > "%TEMP%\CreateShortcut.vbs"
echo sLinkFile = "%SHORTCUT_PATH%" >> "%TEMP%\CreateShortcut.vbs"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%TEMP%\CreateShortcut.vbs"
echo oLink.TargetPath = "%INSTALL_DIR%\StartHRSystem.bat" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.WorkingDirectory = "%INSTALL_DIR%" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Description = "HR System" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.IconLocation = "%INSTALL_DIR%\HRSystem.exe, 0" >> "%TEMP%\CreateShortcut.vbs"
echo oLink.Save >> "%TEMP%\CreateShortcut.vbs"
cscript //nologo "%TEMP%\CreateShortcut.vbs"
del "%TEMP%\CreateShortcut.vbs"
echo    OK: Desktop shortcut created

echo.
echo ========================================
echo    Installation completed successfully!
echo ========================================
echo.
echo Installation path: C:\HRSystem
echo Database server: %SQL_INSTANCE%
echo.
echo ----------------------------------------
echo On first run, the database and tables
echo will be created automatically.
echo ----------------------------------------
echo.
set /p RUN_NOW="Do you want to start the system now? (Y/N): "
if /i "%RUN_NOW%"=="Y" (
    echo.
    echo Starting the system...
    cd /d "%INSTALL_DIR%"
    call StartHRSystem.bat
) else (
    echo.
    echo To start the system:
    echo - Double-click the "HR System" shortcut on desktop
    echo - Or run StartHRSystem.bat
    echo.
    pause
)
