@echo off
setlocal

REM OpenRose - Requirements Management
REM Licensed under the Apache License, Version 2.0.
REM See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

REM This script starts OpenRose.WebUI and, if available, OpenRose.API.
REM If the API is missing, a warning is shown but WebUI still starts normally.
REM It dynamically finds directories containing "OpenRose.API.Exe" and "OpenRose.WebUI.Exe".
REM The script assumes that the applications are located in the same directory as the script.
REM Searches for executables in any sibling directory of the parent folder.

REM DESIGNED TO HELP USERS TO RUN OpenRose as a standalone application.


set "parentDir=%~dp0.."

REM Reset variables
set "apidir="
set "wuidir="

REM Search for OpenRose.API.exe
for /d %%d in ("%parentDir%\*") do (
    if exist "%%d\OpenRose.API.exe" (
        set "apidir=%%d"
    )
)

REM Search for OpenRose.WebUI.exe
for /d %%d in ("%parentDir%\*") do (
    if exist "%%d\OpenRose.WebUI.exe" (
        set "wuidir=%%d"
    )
)

REM Validate WebUI (required)
if not defined wuidir (
    echo ERROR: Could not find OpenRose.WebUI.exe in any sibling directory.
    echo WebUI is required to run OpenRose.
    exit /b
)

REM API optional
if not defined apidir (
    echo WARNING: OpenRose.API.exe not found.
    echo WebUI will start without API support.
) else (
    echo Found API at: %apidir%
)

echo Found WebUI at: %wuidir%
echo.

REM Start API only if present
if defined apidir (
    echo Starting OpenRose.API...
    start "" cmd /c "cd /d %apidir% && start OpenRose.API.exe"
)

REM Start WebUI (always)
echo Starting OpenRose.WebUI...
start "" cmd /c "cd /d %wuidir% && start OpenRose.WebUI.exe"

endlocal
