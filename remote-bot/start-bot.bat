@echo off
title Capybara Remote Bot
cd /d "%~dp0"
:loop
echo Starting Claude remote bot...
node bot.js
echo.
echo Bot stopped (exit code %errorlevel%). Restarting in 5s... (close this window to stop)
timeout /t 5 /nobreak >nul
goto loop
