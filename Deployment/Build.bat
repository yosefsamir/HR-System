@echo off
chcp 65001 >nul
echo Building HR System for Production...
echo.

cd /d "%~dp0\..\HR-system"

REM Clean previous builds
echo [1/4] تنظيف الملفات السابقة...
dotnet clean -c Release >nul 2>&1
if exist "..\Deployment\publish" rmdir /s /q "..\Deployment\publish"

REM Restore packages
echo [2/4] استعادة الحزم...
dotnet restore

REM Build for production
echo [3/4] بناء التطبيق...
dotnet publish -c Release -o "..\Deployment\publish" --self-contained false

REM Copy deployment scripts
echo [4/4] نسخ ملفات التشغيل...
copy "..\Deployment\StartHRSystem.bat" "..\Deployment\publish\" >nul
copy "..\Deployment\StopHRSystem.bat" "..\Deployment\publish\" >nul
copy "..\Deployment\UpdateHRSystem.bat" "..\Deployment\publish\" >nul
copy "..\Deployment\Install.bat" "..\Deployment\publish\" >nul

REM Create Update folder
if not exist "..\Deployment\publish\Update" mkdir "..\Deployment\publish\Update"
if not exist "..\Deployment\publish\Backup" mkdir "..\Deployment\publish\Backup"

echo.
echo ========================================
echo    تم بناء التطبيق بنجاح!
echo    Build completed successfully!
echo ========================================
echo.
echo الملفات موجودة في: Deployment\publish
echo Files are in: Deployment\publish
echo.
echo قم بنسخ مجلد publish إلى جهاز العميل
echo Copy the publish folder to the client machine
echo.
pause
