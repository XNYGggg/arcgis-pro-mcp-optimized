#requires -Version 5.1
<#
卸载客户版组件：
- 仅移除可确认归属本客户版的文件（依据 installed.json 与 add-in 哈希）
- add-in 与运行时默认移动到 .bak / .uninstalled 备份，不直接删除
- 可选清理 Claude Desktop 的 arcgis 条目（带备份）与 Claude Code 的 arcgis MCP
不会删除 ArcGIS Pro 本身与用户工程数据。
参数：-KeepClientConfig 保留客户端配置；-Purge 直接删除不保留备份；-Force 跳过归属校验
#>
[CmdletBinding()]
param(
    [switch]$KeepClientConfig,
    [switch]$Purge,
    [switch]$Force,
    [switch]$Yes
)

$ErrorActionPreference = 'Continue'
function Ok($t)   { Write-Host ('   [OK] ' + $t) -ForegroundColor Green }
function Warn($t) { Write-Host ('   [!]  ' + $t) -ForegroundColor Yellow }
function Fail($t) { Write-Host ('   [X]  ' + $t) -ForegroundColor Red }

$addin = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX'
$runtimeDst = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP'
$markerPath = Join-Path $runtimeDst 'installed.json'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

Write-Host 'ArcGIS Pro MCP 优化版 客户版 · 卸载'

$marker = $null
$owned = $false
if (Test-Path -LiteralPath $markerPath) {
    try {
        $marker = [IO.File]::ReadAllText($markerPath) | ConvertFrom-Json
        if ($marker.package -like 'ArcGIS Pro MCP*') { $owned = $true }
    } catch { }
}
if (-not $owned -and -not $Force) {
    Warn '未发现本客户版的 installed.json 安装标记，无法确认归属。'
    Warn '如确认要卸载，请加 -Force 重新运行。'
    exit 1
}

if (-not $Yes) {
    $ans = Read-Host '确认卸载 add-in 与运行时？(y/N)'
    if ($ans -notmatch '^(y|Y)') { Write-Host '已取消'; exit 0 }
}

# ---- add-in ----
if (Test-Path -LiteralPath $addin) {
    $hash = (Get-FileHash -LiteralPath $addin -Algorithm SHA256).Hash
    $expect = if ($marker) { [string]$marker.addin_sha256 } else { '' }
    if ($Force -or ($expect -and $hash -eq $expect)) {
        if ($Purge) {
            Remove-Item -LiteralPath $addin -Force
            Ok '已移除 add-in（重启 ArcGIS Pro 后生效）'
        } else {
            $bak = $addin + '.uninstalled-' + $stamp
            Move-Item -LiteralPath $addin -Destination $bak -Force
            Ok ('add-in 已移入备份：' + $bak)
        }
    } else {
        Warn 'add-in 哈希与安装标记不一致（可能被其他版本替换），保留未动；加 -Force 可强制移除。'
    }
} else {
    Warn 'add-in 未安装，跳过'
}

# ---- runtime ----
if (Test-Path -LiteralPath $runtimeDst) {
    if ($owned -or $Force) {
        if ($Purge) {
            Remove-Item -LiteralPath $runtimeDst -Recurse -Force
            Ok ('已删除运行时目录：' + $runtimeDst)
        } else {
            $bak = $runtimeDst + '.uninstalled-' + $stamp
            Move-Item -LiteralPath $runtimeDst -Destination $bak -Force
            Ok ('运行时已移入备份：' + $bak)
        }
    } else {
        Warn '运行时目录不含本客户版安装标记，保留未动。'
    }
} else {
    Warn '运行时目录不存在，跳过'
}

# ---- 客户端配置 ----
if (-not $KeepClientConfig) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    foreach ($cfg in @(
        (Join-Path $env:APPDATA 'Claude\claude_desktop_config.json'),
        (Join-Path $env:APPDATA 'Claude-3p\claude_desktop_config.json'),
        (Join-Path $env:LOCALAPPDATA 'Claude-3p\claude_desktop_config.json'),
        (Join-Path $env:LOCALAPPDATA 'Claude\claude_desktop_config.json'))) {
        if (-not (Test-Path -LiteralPath $cfg)) { continue }
        try {
            $json = [IO.File]::ReadAllText($cfg) | ConvertFrom-Json
            if ($null -ne $json.PSObject.Properties['mcpServers'] -and
                $null -ne $json.mcpServers.PSObject.Properties['arcgis']) {
                Copy-Item -LiteralPath $cfg -Destination ($cfg + '.bak-' + $stamp) -Force
                $json.mcpServers.PSObject.Properties.Remove('arcgis')
                [IO.File]::WriteAllText($cfg, ($json | ConvertTo-Json -Depth 30), $utf8NoBom)
                Ok ('已从 Claude Desktop 配置移除 arcgis：' + $cfg)
            }
        } catch { Warn ('清理失败：' + $cfg) }
    }
    if (Get-Command claude -ErrorAction SilentlyContinue) {
        & claude mcp remove arcgis 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { Ok '已从 Claude Code 移除 arcgis' } else { Warn 'Claude Code 中未找到 arcgis 条目或移除失败' }
    }
}

Write-Host '卸载完成。'
exit 0
