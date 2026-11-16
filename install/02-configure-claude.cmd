@echo off
chcp 65001 >nul
title ArcGIS Pro MCP Customer Edition - Configure Claude
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0configure-claude.ps1" %*
echo.
pause
