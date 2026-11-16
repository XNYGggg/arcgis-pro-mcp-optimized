@echo off
chcp 65001 >nul
title ArcGIS Pro MCP Customer Edition - Dialog Guard
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0dialog-guard.ps1" %*
echo.
pause
