@ECHO OFF
setlocal
ECHO ================================================= 
echo Overlay Enabler Starter
ECHO ================================================= 
echo.
echo Press Ctrl + E to exit, Insert to toggle show/hide, Ctrl + R to reload page.
echo.
setlocal enableDelayedExpansion 
REM Ask if using Spectra Overlay
choice /C YN /M "Are you using Spectra Overlay?"

if errorlevel 2 (
    REM Normal
    REM Prompt the user for a URL
    set /p url="Please enter the URL: "
    choice /C YN /M "Do you want to use subURL ?"
    if errorlevel 2 (
            START OverlayEnabler.exe !url!
            timeout /t 5
            exit /b 0
    ) else (
        call:setupsubURL
        timeout /t 5
        exit /b 0
    ) 
) else (
    REM Spectra Overlay
    REM Prompt the user for a URL
    set /p url="Please enter the URL (eg: https://eu.valospectra.com): "
    set /p groupCode="Please enter the Group code: "
    ECHO ======================================================================================================== 
    echo Press Ctrl + F2 for technical pause, Ctrl + F3 for Left team timeout, Ctrl + F4 for Right team timeout.
    echo Press Ctrl + F1 to hide all timeout/pause.
    ECHO ======================================================================================================== 
    START OverlayEnabler.exe "!url!/overlay?groupCode=!groupCode!" "!url!/timeout?groupCode=!groupCode!&team=tech" "!url!/timeout?groupCode=!groupCode!&team=1" "!url!/timeout?groupCode=!groupCode!&team=2"
    timeout /t 5
    exit /b 0
)

:setupsubURL
set /p subURLTotal="How many subURL(s) do you want to use? (1-5): "
    if %subURLTotal% LSS 1 goto :error
    if %subURLTotal% GTR 5 goto :error
    for /L %%i in (1,1,%subURLTotal%) do (
            set /p subURLtemp="Enter subURL %%i: "
            if %%i == 1 (
                set subURL= "!subURLtemp!"
            ) else (
            set subURL= !subURL! "!subURLtemp!"
        )
    )
    START OverlayEnabler.exe !url! !subURL!
goto:eof

:error
echo Error: Please enter a number between 1 and 5
exit /b 1
goto:eof

endlocal