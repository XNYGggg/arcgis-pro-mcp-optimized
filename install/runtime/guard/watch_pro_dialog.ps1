#requires -Version 5.1
<#
兼容层：旧 watch_pro_dialog.ps1 参数原样转发到 Guard 2.0 引擎。
- 参数保持不变：-DurationSeconds / -AlertOnly / -NotifyTest / -TestSeconds /
  -AlertMinSeconds / -SelfTest
- 新引擎会生成 manual 令牌，按 DurationSeconds 超时退出；日志写入
  %LOCALAPPDATA%\ArcGIS-AgentKit\logs\guard.jsonl
#>
param(
    [int]$DurationSeconds = 600,
    [switch]$AlertOnly,
    [switch]$NotifyTest,
    [int]$TestSeconds = 30,
    [int]$AlertMinSeconds = 30,
    [switch]$SelfTest
)

$ErrorActionPreference = 'Stop'
$engine = Join-Path $PSScriptRoot 'guard.ps1'
if (-not (Test-Path -LiteralPath $engine)) {
    Write-Host ('守卫引擎不存在：' + $engine) -ForegroundColor Red
    exit 1
}

& $engine @PSBoundParameters
exit $LASTEXITCODE
