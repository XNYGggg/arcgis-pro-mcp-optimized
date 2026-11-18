#requires -Version 5.1
<#
客户版自检：Pro 版本 / add-in / 运行时 / 端口 / 客户端配置
输出逐项 [OK]/[!]/[X]；退出码 0=全部通过，1=有未通过项
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
function Ok($t)   { Write-Host ('   [OK] ' + $t) -ForegroundColor Green; $script:pass++ }
function Warn($t) { Write-Host ('   [!]  ' + $t) -ForegroundColor Yellow; $script:warn++ }
function Fail($t) { Write-Host ('   [X]  ' + $t) -ForegroundColor Red; $script:fail++ }
function Step($t) { Write-Host ''; Write-Host ('== ' + $t) -ForegroundColor Cyan }

$script:pass = 0; $script:warn = 0; $script:fail = 0
$runtimeDst = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP'

Write-Host 'ArcGIS Pro MCP 优化版 客户版 Customer 1.16（1.14 增强版） · 自检'

Step 'ArcGIS Pro'
$proExe = $null
try {
    $dir = (Get-ItemProperty 'HKLM:\SOFTWARE\ESRI\ArcGISPro' -ErrorAction SilentlyContinue).InstallDir
    if ($dir) { $c = Join-Path $dir 'bin\ArcGISPro.exe'; if (Test-Path -LiteralPath $c) { $proExe = $c } }
} catch { }
if (-not $proExe) {
    foreach ($c in @('C:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe',
                     'D:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe',
                     'E:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe')) {
        if (Test-Path -LiteralPath $c) { $proExe = $c; break }
    }
}
if ($proExe) {
    $verText = (Get-Item $proExe).VersionInfo.ProductVersion
    $ver = [version]($verText.Split(' ')[0])
    if ($ver -ge [version]'3.7') { Ok ('ArcGIS Pro ' + $ver) } else { Fail ('ArcGIS Pro ' + $ver + ' 低于 3.7') }
} else {
    Fail '未找到 ArcGIS Pro'
}
$proRunning = $null -ne (Get-Process -Name ArcGISPro -ErrorAction SilentlyContinue)
if ($proRunning) { Ok 'ArcGIS Pro 正在运行' } else { Warn 'ArcGIS Pro 未运行（桥接与端口检查会失败）' }

Step '.NET Desktop Runtime 10'
$desktop10 = @()
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    try {
        $desktop10 = @(& dotnet --list-runtimes 2>$null | Where-Object { $_ -match '^Microsoft\.WindowsDesktop\.App\s+10\.' } | ForEach-Object { ($_ -replace '\s+', ' ').Trim() })
    } catch { }
}
if ($desktop10.Count -eq 0) {
    $base = Join-Path $env:ProgramFiles 'dotnet\shared\Microsoft.WindowsDesktop.App'
    if (Test-Path -LiteralPath $base) {
        $desktop10 = @(Get-ChildItem -LiteralPath $base -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '10.*' } | ForEach-Object { $_.Name })
    }
}
if ($desktop10.Count -gt 0) { Ok ('已检测到：' + ($desktop10 -join '; ')) }
else { Warn '未自动检测到 .NET Desktop Runtime 10（Pro 3.7 的受支持安装通常自带；缺失时 add-in 无法加载）' }

Step '端口占用（6520 / 6510 / 6511）'
foreach ($port in @(6520, 6510, 6511)) {
    $line = $null
    try {
        $conn = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($conn) {
            $p = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            $line = $(if ($p) { $p.ProcessName + ' (PID ' + $p.Id + ')' } else { 'PID ' + $conn.OwningProcess })
        }
    } catch {
        $netLine = (netstat -ano | Select-String (':' + $port + '\s+') | Select-Object -First 1)
        if ($netLine) { $line = $netLine.Line.Trim() }
    }
    if ($line) { Warn ('端口 ' + $port + ' 已被占用：' + $line + '（若为 ArcGISPro 则属正常）') }
    else { Ok ('端口 ' + $port + ' 空闲') }
}

Step '文档目录'
$docDir = [Environment]::GetFolderPath('MyDocuments')
if ($docDir -match 'OneDrive') { Warn ('文档目录已被 OneDrive 重定向：' + $docDir + '；ArcGIS Pro 使用同一目录，通常无需处理') }
else { Ok ('文档目录：' + $docDir) }

Step 'add-in'
$addin = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX'
if (Test-Path -LiteralPath $addin) {
    Ok ('已安装：' + $addin)
    $side = Join-Path $PSScriptRoot 'addin.sha256'
    if (Test-Path -LiteralPath $side) {
        $want = (Get-Content -LiteralPath $side -TotalCount 1).Trim().Split(' ')[0].ToLower()
        $got = (Get-FileHash -LiteralPath $addin -Algorithm SHA256).Hash.ToLower()
        if ($want -eq $got) { Ok ('SHA256 与客户包一致：' + $got) } else { Warn ('SHA256 不一致（可能已被其他版本覆盖）：' + $got) }
    }
} else {
    Fail 'add-in 未安装，请先运行 00-install.cmd'
}

Step '运行时'
foreach ($item in @(
    @{ p = (Join-Path $runtimeDst 'stdio-bridge.ps1'); n = 'stdio 桥' },
    @{ p = (Join-Path $runtimeDst 'runcode\RunCode.pyt'); n = 'RunCode 工具箱' },
    @{ p = (Join-Path $runtimeDst 'guard\watch_pro_dialog.ps1'); n = '弹窗守护' },
    @{ p = (Join-Path $runtimeDst 'plugin\mcp_bridge.py'); n = 'Python 桥' })) {
    if (Test-Path -LiteralPath $item.p) { Ok $item.n } else { Fail ('缺少 ' + $item.n + '：' + $item.p) }
}

Step 'MCP 桥接端口 127.0.0.1:6520'
$tcpOk = $false
try {
    $client = New-Object System.Net.Sockets.TcpClient
    $iar = $client.BeginConnect('127.0.0.1', 6520, $null, $null)
    if ($iar.AsyncWaitHandle.WaitOne(1500)) { $client.EndConnect($iar); $tcpOk = $client.Connected }
    $client.Close()
} catch { }
if ($tcpOk) { Ok '端口在监听' } else { Warn '端口未监听（请先打开 Pro 并确认 MCP 选项卡显示 running）' }

if ($tcpOk) {
    try {
        $headers = @{ Accept = 'application/json, text/event-stream' }
        $init = '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"agentkit-check","version":"1.0"}}}'
        $null = Invoke-RestMethod -Uri 'http://127.0.0.1:6520/mcp' -Method Post -Body ([Text.Encoding]::UTF8.GetBytes($init)) -ContentType 'application/json; charset=utf-8' -Headers $headers -TimeoutSec 10
        Ok 'MCP initialize 成功'
    } catch {
        Fail ('MCP HTTP 无响应：' + $_.Exception.Message)
    }
}

Step '客户端配置'
$configs = @(
    (Join-Path $env:APPDATA 'Claude\claude_desktop_config.json'),
    (Join-Path $env:APPDATA 'Claude-3p\claude_desktop_config.json'),
    (Join-Path $env:LOCALAPPDATA 'Claude-3p\claude_desktop_config.json'),
    (Join-Path $env:LOCALAPPDATA 'Claude\claude_desktop_config.json')
)
$found = $false
foreach ($cfg in $configs) {
    if (-not (Test-Path -LiteralPath $cfg)) { continue }
    try {
        $raw = [IO.File]::ReadAllText($cfg)
        if ($raw -match '"arcgis"') { Ok ('Claude Desktop 已配置：' + $cfg); $found = $true }
    } catch { }
}
if (-not $found) { Warn '未在任何 Claude Desktop 配置中发现 arcgis 条目（可运行 02-configure-claude.cmd）' }
if (Get-Command claude -ErrorAction SilentlyContinue) {
    try {
        $list = (& claude mcp list 2>$null) -join ' '
        if ($list -match 'arcgis') { Ok 'Claude Code 已配置 arcgis' } else { Warn 'Claude Code 未配置 arcgis' }
    } catch { }
}

Write-Host ''
Write-Host ('结果：' + $script:pass + ' 通过 / ' + $script:warn + ' 警告 / ' + $script:fail + ' 失败')
if ($script:fail -gt 0) { exit 1 } else { exit 0 }
