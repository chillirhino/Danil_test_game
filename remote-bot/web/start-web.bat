@echo off
title Capybara Web Console
cd /d "%~dp0"
:loop
echo Starting web console...
node server.js
echo.
echo Server stopped (exit %errorlevel%). Restarting in 5s... (close window to stop)
timeout /t 5 /nobreak >nul
goto loop
