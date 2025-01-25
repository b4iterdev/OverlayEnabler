@echo off
setlocal

echo Overlay Enabler Starter
echo.
echo Press Ctrl + E to exit, Insert to toggle show/hide, Ctrl + R to reload page.

REM Prompt the user for a URL
set /p url="Please enter the URL: "

REM Execute OverlayEnabler.exe with the provided URL
OverlayEnabler.exe %url%

endlocal
pause