#requires -Version 5.1
<#
按需弹窗守卫启动器（Guard 2.0，可选组件）
- 在 ArcGIS Pro 可能弹出“第三方代码库 Python 命令执行”确认框时，
  自动点击“是/允许”；点不掉则弹置顶提醒窗（不少于 30 秒、每 10 秒响铃）。
- 2.0 起 add-in 会在 run_python_toolbox_tool 前后自动召唤/收队，本启动器用于
  手工步骤（如 ENVI→Pro 导入、自己跑脚本）。
- 用法：
    03-dialog-guard.cmd                启动 600 秒守卫
    03-dialog-guard.cmd -DurationSeconds 120
    03-dialog-guard.cmd -NotifyTest    只做提醒窗可见性测试（30 秒）
    03-dialog-guard.cmd -AlertOnly     只提醒、不自动点击
    03-dialog-guard.cmd -SelfTest      扫描当前 Pro 窗口并输出诊断
- 安全边界：只处理“第三方代码库”信任框，绝不触碰覆盖/删除等其他确认框。
#>
[CmdletBinding()]
param(
    [int]$DurationSeconds = 600,
    [switch]$AlertOnly,
    [switch]$NotifyTest,
    [switch]$SelfTest,
    [int]$TestSeconds = 30,
    [int]$AlertMinSeconds = 30
)

$ErrorActionPreference = 'Stop'
$guard = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP\guard\guard.ps1'
if (-not (Test-Path -LiteralPath $guard)) {
    Write-Host ('守卫引擎不存在：' + $guard) -ForegroundColor Red
    Write-Host '请先运行 00-install.cmd' -ForegroundColor Yellow
    exit 1
}

# 路径可能含空格，必须带引号拼进命令行（Start-Process 的 -ArgumentList 不做自动转义）
$argsLine = '-NoProfile -ExecutionPolicy Bypass -File "' + $guard + '"'
if ($NotifyTest) {
    $argsLine += ' -NotifyTest -TestSeconds ' + [Math]::Max(30, $TestSeconds)
    Write-Host '正在启动提醒窗可见性测试（至少 30 秒后自动关闭）...' -ForegroundColor Cyan
} elseif ($SelfTest) {
    $argsLine += ' -SelfTest'
    Write-Host '正在扫描当前 ArcGIS Pro 窗口并输出守卫诊断...' -ForegroundColor Cyan
} else {
    $argsLine += ' -Scope manual -DurationSeconds ' + $DurationSeconds
    $argsLine += ' -AlertMinSeconds ' + [Math]::Max(30, $AlertMinSeconds)
    if ($AlertOnly) { $argsLine += ' -AlertOnly' }
    Write-Host ('正在启动按需弹窗守卫（' + $DurationSeconds + ' 秒' + $(if ($AlertOnly) { '，仅提醒模式' } else { '' }) + '）...') -ForegroundColor Cyan
}
$log = Join-Path $env:LOCALAPPDATA 'ArcGIS-AgentKit\logs\guard.jsonl'
Write-Host '守卫在后台运行；日志：' + $log
Start-Process -FilePath 'powershell.exe' -ArgumentList $argsLine -WindowStyle Hidden
exit 0
