@echo off
chcp 65001 >nul
title ArcGIS Pro MCP Customer Edition - Install
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
echo.
pause
