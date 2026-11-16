#requires -Version 5.1
<#
为 Claude Desktop / Claude Code 写入 arcgis MCP 配置。
- Claude Desktop：向已存在的配置文件写入 mcpServers.arcgis（stdio → PowerShell 桥）
- 配置文件解析失败：立即停止并保留原文件，绝不重建（fail-closed）
- Claude Code：claude mcp remove / add，并检查退出码
每个被修改的配置文件都会自动备份。
#>
[CmdletBinding()]
param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Continue'

function Info($t) { if (-not $Quiet) { Write-Host ('   ' + $t) } }
function Ok($t)   { Write-Host ('   [OK] ' + $t) -ForegroundColor Green }
function Warn($t) { Write-Host ('   [!]  ' + $t) -ForegroundColor Yellow }

$url       = 'http://127.0.0.1:6520/mcp'
$bridgeDir = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP'
$bridge    = Join-Path $bridgeDir 'stdio-bridge.ps1'
$psExe     = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
if (-not (Test-Path -LiteralPath $psExe)) { $psExe = 'powershell.exe' }
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

if (-not (Test-Path -LiteralPath $bridge)) {
    Warn ('stdio 桥不存在：' + $bridge + '（请先运行 00-install.cmd）')
    exit 1
}

$claudeRunning = $null -ne (Get-Process -Name Claude -ErrorAction SilentlyContinue)
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$candidates = @(
    (Join-Path $env:APPDATA 'Claude'),
    (Join-Path $env:APPDATA 'Claude-3p'),
    (Join-Path $env:LOCALAPPDATA 'Claude-3p'),
    (Join-Path $env:LOCALAPPDATA 'Claude')
)
$written = 0
$hadError = $false

function Set-ArcGISEntry([string]$cfgPath) {
    $json = $null
    $raw = [IO.File]::ReadAllText($cfgPath)
    if ([string]::IsNullOrWhiteSpace($raw)) {
        $json = New-Object psobject
    } else {
        # 解析失败时向外抛出，由调用方中止（fail-closed，绝不重建）
        $json = $raw | ConvertFrom-Json
    }
    if ($null -eq $json.PSObject.Properties['mcpServers']) {
        $json | Add-Member -NotePropertyName mcpServers -NotePropertyValue (New-Object psobject)
    }
    $serverObj = [pscustomobject]@{
        command = $psExe
        args    = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $bridge)
    }
    if ($null -eq $json.mcpServers.PSObject.Properties['arcgis']) {
        $json.mcpServers | Add-Member -NotePropertyName arcgis -NotePropertyValue $serverObj
    } else {
        $json.mcpServers.arcgis = $serverObj
    }
    [IO.File]::WriteAllText($cfgPath, ($json | ConvertTo-Json -Depth 30), $utf8NoBom)
}

foreach ($dir in $candidates) {
    if (-not (Test-Path -LiteralPath $dir)) { continue }
    $cfg = Join-Path $dir 'claude_desktop_config.json'
    if (-not (Test-Path -LiteralPath $cfg)) { continue }
    if ($claudeRunning) {
        Warn ('Claude Desktop 正在运行，跳过写入 ' + $cfg)
        Warn '请先完全退出 Claude Desktop，再运行 02-configure-claude.cmd'
        continue
    }
    try {
        # 先验证可解析，再备份、再写入；解析失败直接中止本文件
        $null = [IO.File]::ReadAllText($cfg) | ConvertFrom-Json
        Copy-Item -LiteralPath $cfg -Destination ($cfg + '.bak-' + $stamp) -Force
        Set-ArcGISEntry $cfg
        Ok ('Claude Desktop 配置已写入：' + $cfg)
        $written++
    } catch {
        $hadError = $true
        Warn ('配置文件解析或写入失败，已跳过（原文件未动）：' + $cfg)
        Warn ('原因：' + $_.Exception.Message + '；请手工修复 JSON 后重试')
    }
}

if ($written -eq 0 -and -not $claudeRunning -and -not $hadError) {
    $dir = Join-Path $env:APPDATA 'Claude'
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $cfg = Join-Path $dir 'claude_desktop_config.json'
    if (-not (Test-Path -LiteralPath $cfg)) {
        $json = New-Object psobject
        $json | Add-Member -NotePropertyName mcpServers -NotePropertyValue (New-Object psobject)
        $json.mcpServers | Add-Member -NotePropertyName arcgis -NotePropertyValue ([pscustomobject]@{
            command = $psExe
            args    = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $bridge)
        })
        [IO.File]::WriteAllText($cfg, ($json | ConvertTo-Json -Depth 30), $utf8NoBom)
        Ok ('未发现现有 Claude Desktop 配置，已创建：' + $cfg)
        $written++
    }
}
if ($written -eq 0 -and $claudeRunning) {
    Warn '未写入任何 Claude Desktop 配置（应用正在运行）。关闭应用后重跑本脚本即可。'
}

# ---------- Claude Code ----------
$claudeCmd = Get-Command claude -ErrorAction SilentlyContinue
if ($claudeCmd) {
    & claude mcp remove arcgis 2>$null | Out-Null
    $removeExit = $LASTEXITCODE
    & claude mcp add --transport http arcgis $url | Out-Null
    $addExit = $LASTEXITCODE
    if ($addExit -eq 0) {
        Ok ('Claude Code 已接入：' + $url)
    } else {
        $hadError = $true
        Warn ('Claude Code 配置失败（add 退出码 ' + $addExit + '，remove 退出码 ' + $removeExit + '）')
        Warn '请手工执行：claude mcp remove arcgis; claude mcp add --transport http arcgis ' + $url
    }
} else {
    Info '未检测到 claude 命令，跳过 Claude Code 配置（详见 clients\claude-code.md）'
}

if ($hadError) { exit 1 } else { exit 0 }
