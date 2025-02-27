@echo off
setlocal enabledelayedexpansion

:: Initialize port number and PID list
set PORT=9222
set PID_LIST=

:: Get the list of all forwarded ports and PIDs
for /f "tokens=2,5 delims=:_" %%a in ('hdc fport ls') do (
    if %%a gtr !PORT! (
        set PORT=%%a
    )
    for /f "tokens=1 delims= " %%c in ("%%b") do (
        set PID_LIST=!PID_LIST! %%c
    )
)

:: Increment port number for next application
set temp_PORT=!PORT!
set /a temp_PORT+=1
set PORT=!temp_PORT!

:: Get the domain socket name of devtools
for /f "tokens=*" %%a in ('hdc shell "cat /proc/net/unix | grep devtools"') do (
    set SOCKET_NAME=%%a

    :: Extract process ID
    for /f "delims=_ tokens=4" %%b in ("!SOCKET_NAME!") do set PID=%%b

    :: Check if PID already has a mapping
    echo !PID_LIST! | findstr /C:" !PID! " >nul
    if errorlevel 1 (
        :: Add mapping
        hdc fport tcp:!PORT! localabstract:webview_devtools_remote_!PID!
        if errorlevel 1 (
            echo Error: Failed to add mapping.
            pause
            exit /b
        )

        :: Add PID to list and increment port number for next application
        set PID_LIST=!PID_LIST! !PID!
        set temp_PORT=!PORT!
        set /a temp_PORT+=1
        set PORT=!temp_PORT!
    )
)

:: If no process ID was found, prompt the user to open debugging in their application code and provide the documentation link
if "!SOCKET_NAME!"=="" (
    echo No process ID was found. Please open debugging in your application code using the corresponding interface. You can find the relevant documentation at this link: [https://gitee.com/openharmony/docs/blob/master/zh-cn/application-dev/web/web-debugging-with-devtools.md]
    pause
    exit /b
)

:: Check mapping
hdc fport ls

echo.
echo Script executed successfully. Press any key to exit...
pause >nul

:: Try to open the page in Edge
start msedge chrome://inspect/#devices.com

:: If Edge is not available, then open the page in Chrome
if errorlevel 1 (
    start chrome chrome://inspect/#devices.com
)

endlocal