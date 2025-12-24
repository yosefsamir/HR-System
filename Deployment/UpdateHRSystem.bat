@echo off
title HR System Update

echo ========================================
echo         HR System Update
echo ========================================
echo.
echo WARNING: System will be stopped temporarily for update
echo Data will NOT be affected - only program files
echo.
pause

cd /d "%~dp0"

REM Stop the running application
echo.
echo [1/4] Stopping current system...
taskkill /IM "HRSystem.exe" /F 2>nul
timeout /t 2 /nobreak >nul

REM Backup current version
echo.
echo [2/4] Backing up current version...
if not exist "Backup" mkdir Backup
set BACKUP_NAME=Backup\backup_%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%
set BACKUP_NAME=%BACKUP_NAME: =0%
if exist "HRSystem.exe" copy "HRSystem.exe" "%BACKUP_NAME%_HRSystem.exe" >nul
echo    OK: Backup created

REM Copy new files from Update folder
echo.
echo [3/4] Copying new files...
if exist "Update\*" (
    xcopy /E /Y "Update\*" "." >nul
    echo    OK: Update files copied
) else (
    echo    ERROR: Update folder not found or empty
    echo    Please put update files in the Update folder
    pause
    exit /b 1
)

REM Apply database migrations
echo.
echo [4/4] Updating database...
if exist "HRSystem.exe" (
    HRSystem.exe --migrate 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo    WARNING: You may need to update the database manually
    ) else (
        echo    OK: Database updated
    )
)

echo.
echo ========================================
echo    Update completed successfully!
echo ========================================
echo.
echo You can now start the system.
echo.
pause
