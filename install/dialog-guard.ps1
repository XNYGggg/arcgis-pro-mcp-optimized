#requires -Version 5.1
<#
弹窗守护启动器（可选组件）
- 在 ArcGIS Pro 可能弹出“第三方代码库 Python 命令执行”确认框时，
  自动点击“是/允许”；点不掉则弹置顶提醒窗（不少于 30 秒、每 10 秒响铃）。
- 用法：
    03-dialog-guard.cmd                启动 600 秒守护
    03-dialog-guard.cmd -DurationSeconds 120
    03-dialog-guard.cmd -NotifyTest    只做提醒窗可见性测试（30 秒）
    03-dialog-guard.cmd -AlertOnly     只提醒、不自动点击
- 安全边界：只处理“第三方代码库”信任框，绝不触碰覆盖/删除等其他确认框。
#>
[CmdletBinding()]
param(
    [int]$DurationSeconds = 600,
    [switch]$AlertOnly,
    [switch]$NotifyTest
)

$ErrorActionPreference = 'Stop'
$guard = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP\guard\watch_pro_dialog.ps1'
if (-not (Test-Path -LiteralPath $guard)) {
    Write-Host ('守护脚本不存在：' + $guard) -ForegroundColor Red
    Write-Host '请先运行 00-install.cmd' -ForegroundColor Yellow
    exit 1
}

# 路径可能含空格，必须带引号拼进命令行（Start-Process 的 -ArgumentList 不做自动转义）
$argsLine = '-NoProfile -ExecutionPolicy Bypass -File "' + $guard + '"'
if ($NotifyTest) {
    $argsLine += ' -NotifyTest -TestSeconds 30'
    Write-Host '正在启动提醒窗可见性测试（30 秒后自动关闭）...' -ForegroundColor Cyan
} else {
    $argsLine += ' -DurationSeconds ' + $DurationSeconds
    if ($AlertOnly) { $argsLine += ' -AlertOnly' }
    Write-Host ('正在启动弹窗守护（' + $DurationSeconds + ' 秒' + $(if ($AlertOnly) { '，仅提醒模式' } else { '' }) + '）...') -ForegroundColor Cyan
}
Write-Host '守护在后台运行；日志：' + (Join-Path (Split-Path $guard -Parent) 'watch_pro_dialog.log')
Start-Process -FilePath 'powershell.exe' -ArgumentList $argsLine -WindowStyle Hidden
exit 0
