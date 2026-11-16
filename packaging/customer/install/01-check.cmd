@echo off
chcp 65001 >nul
title ArcGIS Pro MCP Customer Edition - Check
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0check.ps1" %*
echo.
pause
