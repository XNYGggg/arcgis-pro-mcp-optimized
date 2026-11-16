@echo off
chcp 65001 >nul
title ArcGIS Pro MCP Customer Edition - Uninstall
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1" %*
echo.
pause
