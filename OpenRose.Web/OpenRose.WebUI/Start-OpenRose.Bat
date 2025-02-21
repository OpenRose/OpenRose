
REM OpenRose - Requirements Management
REM Licensed under the Apache License, Version 2.0. 
REM See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

REM This script starts the OpenRose.API and OpenRose.WebUI applications in separate command windows.
REM The script searches for the directories starting with OpenRose.API and OpenRose.WebUI in the parent directory of the script.
REM The script assumes that the applications are located in the same directory as the script.
REM DESIGNED TO HELP USERS TO RUN OpenRose as a standalone application.

@echo off
setlocal

REM Find the directory starting with OpenRose.API
for /d %%d in (..\OpenRose.API*) do (
    set apidir=%%d
)

REM Find the directory starting with OpenRose.WebUI
for /d %%d in (..\OpenRose.WebUI*) do (
    set wuidir=%%d
)

REM Start the applications in new command windows
start "" cmd /c "cd /d %apidir% && start OpenRose.API.exe"
start "" cmd /c "cd /d %wuidir% && start OpenRose.WebUI.exe"

endlocal