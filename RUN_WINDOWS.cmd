@echo off
setlocal
cd /d "%~dp0"

echo ============================================
echo Gift of the Givers - Application Launcher
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: .NET SDK is not installed or is not on PATH.
  echo Install the .NET 8 SDK, then close and reopen Command Prompt.
  echo After installation, run: dotnet --version
  echo.
  pause
  exit /b 1
)

echo Installed SDKs:
dotnet --list-sdks
echo.

echo [1/3] Restoring packages...
dotnet restore GiftOfTheGivers.csproj
if errorlevel 1 goto :failed

echo.
echo [2/3] Building project...
dotnet build GiftOfTheGivers.csproj --no-restore
if errorlevel 1 goto :failed

echo.
echo [3/3] Starting website at http://localhost:5098
echo Press Ctrl+C to stop the website.
echo.
dotnet run --project GiftOfTheGivers.csproj --no-build --launch-profile GiftOfTheGivers
exit /b %errorlevel%

:failed
echo.
echo BUILD FAILED. Review the first error shown above, correct it, and run the launcher again.
pause
exit /b 1
