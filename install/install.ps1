#requires -Version 5.1
<#
ArcGIS Pro MCP 优化版（客户版）安装脚本
- 安装 add-in 到当前用户（无需管理员）
- 部署运行时：stdio 桥 / RunCode 工具箱 / 弹窗守护 / Python 桥
- 原子替换：先复制到暂存并校验，再原子替换；失败自动回滚
- 可选：自动配置 Claude Desktop 与 Claude Code
退出码：0 成功；1 部分失败；2 环境不满足
#>
[CmdletBinding()]
param(
    [switch]$SkipClients,
    [Parameter(ValueFromRemainingArguments = $true)]
    [object[]]$Rest
)

$ErrorActionPreference = 'Stop'

function Step($t) { Write-Host ''; Write-Host ('== ' + $t) -ForegroundColor Cyan }
function Ok($t)   { Write-Host ('   [OK] ' + $t) -ForegroundColor Green }
function Warn($t) { Write-Host ('   [!]  ' + $t) -ForegroundColor Yellow }
function Fail($t) { Write-Host ('   [X]  ' + $t) -ForegroundColor Red }

$pkgRoot    = Split-Path -Parent $PSScriptRoot
$addinSrc   = Join-Path $PSScriptRoot 'ArcGISProMCP.esriAddinX'
$runtimeSrc = Join-Path $PSScriptRoot 'runtime'
$addinsDir  = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'ArcGIS\AddIns\ArcGISPro'
$runtimeDst = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP'
$stamp      = Get-Date -Format 'yyyyMMdd-HHmmss'

Write-Host 'ArcGIS Pro MCP 优化版 · 客户版 Customer 1.1.11（1.14 增强版）' -ForegroundColor White
Write-Host '组件：add-in + stdio 桥 + RunCode + 弹窗守护 + Python 桥'

if ($Rest -and $Rest.Count -gt 0) {
    Warn ('已忽略未知参数：' + ($Rest -join ' ') + '（支持的参数只有 -SkipClients）')
}

# ---------- 1. 检查 ArcGIS Pro ----------
Step '检查 ArcGIS Pro（需要 3.7；仅 3.7 经实机验证）'
$proExe = $null
try {
    $dir = (Get-ItemProperty 'HKLM:\SOFTWARE\ESRI\ArcGISPro' -ErrorAction SilentlyContinue).InstallDir
    if ($dir) {
        $c = Join-Path $dir 'bin\ArcGISPro.exe'
        if (Test-Path -LiteralPath $c) { $proExe = $c }
    }
} catch { }
if (-not $proExe) {
    foreach ($c in @(
        'C:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe',
        'D:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe',
        'E:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe')) {
        if (Test-Path -LiteralPath $c) { $proExe = $c; break }
    }
}
if (-not $proExe) {
    Fail '未找到 ArcGIS Pro。请先安装 ArcGIS Pro 3.7，再运行本安装程序。'
    exit 2
}
$verText = (Get-Item $proExe).VersionInfo.ProductVersion
$ver = [version]($verText.Split(' ')[0])
Ok ('ArcGIS Pro ' + $ver + '  (' + $proExe + ')')
if ($ver -lt [version]'3.7') {
    Fail '本客户版的 add-in 基于 ArcGIS Pro 3.7 / .NET 10 构建，需要 3.7 或更高；本版仅在 3.7 上实机验证。'
    exit 2
}
if ($ver -gt [version]'3.7') {
    Warn '本客户版仅在 ArcGIS Pro 3.7 上实机验证；当前版本高于 3.7，属未验证组合，请自行评估。'
}

# ---------- 2. .NET Desktop Runtime 10（Pro 3.7 的受支持环境自带） ----------
Step '检查 .NET Desktop Runtime 10'
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
if ($desktop10.Count -gt 0) {
    Ok ('已检测到：' + ($desktop10 -join '; '))
} else {
    Warn '未自动检测到 .NET Desktop Runtime 10。ArcGIS Pro 3.7 的受支持安装通常已自带；若 add-in 无法加载，请修复/重装 Pro 3.7。'
}

# ---------- 3. 端口占用（仅提示，不阻塞） ----------
Step '端口占用检查（6520 / 6510 / 6511，仅提示）'
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
    if ($line) {
        Warn ('端口 ' + $port + ' 已被占用：' + $line + '（若为 ArcGISPro 则属正常）')
    } else {
        Ok ('端口 ' + $port + ' 空闲')
    }
}

# ---------- 4. 用户文档目录（OneDrive 重定向提示） ----------
Step '用户文档目录'
$docDir = [Environment]::GetFolderPath('MyDocuments')
if ($docDir -match 'OneDrive') {
    Warn ('文档目录已被 OneDrive 重定向：' + $docDir + '；ArcGIS Pro 使用同一目录，通常无需处理')
} else {
    Ok ('文档目录：' + $docDir)
}

# ---------- 5. 安装 add-in（原子替换） ----------
Step '安装 add-in（当前用户，不需要管理员）'
if (-not (Test-Path -LiteralPath $addinsDir)) {
    New-Item -ItemType Directory -Force -Path $addinsDir | Out-Null
}
$addinDst = Join-Path $addinsDir 'ArcGISProMCP.esriAddinX'
$srcHash = (Get-FileHash -LiteralPath $addinSrc -Algorithm SHA256).Hash
$needCopy = $true
if (Test-Path -LiteralPath $addinDst) {
    if ((Get-FileHash -LiteralPath $addinDst -Algorithm SHA256).Hash -eq $srcHash) {
        $needCopy = $false
        Ok 'add-in 已是最新版本，跳过'
    }
}
if ($needCopy) {
    $tmp = Join-Path $addinsDir ('.ArcGISProMCP.new.' + [guid]::NewGuid().ToString('N'))
    Copy-Item -LiteralPath $addinSrc -Destination $tmp -Force
    if ((Get-FileHash -LiteralPath $tmp -Algorithm SHA256).Hash -ne $srcHash) {
        Remove-Item -LiteralPath $tmp -Force
        Fail 'add-in 复制校验失败，安装中止（未改动现有文件）'
        exit 1
    }
    $bak = $null
    if (Test-Path -LiteralPath $addinDst) {
        $bak = $addinDst + '.bak-' + $stamp
        Move-Item -LiteralPath $addinDst -Destination $bak -Force
    }
    try {
        Move-Item -LiteralPath $tmp -Destination $addinDst -Force
    } catch {
        if ($bak) { Move-Item -LiteralPath $bak -Destination $addinDst -Force }
        Fail ('add-in 替换失败，已回滚：' + $_.Exception.Message)
        exit 1
    }
    if ($bak) { Ok ('已更新（旧版备份：' + (Split-Path -Leaf $bak) + '）') } else { Ok 'add-in 已安装' }
}
Ok ('SHA256 ' + $srcHash)

# ---------- 6. 部署运行时（暂存 → 校验 → 原子替换 → 失败回滚） ----------
Step '部署运行时组件'
$stage = Join-Path $env:LOCALAPPDATA ('ArcGIS Pro MCP.stage.' + $stamp)
if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null
Copy-Item -LiteralPath (Join-Path $runtimeSrc 'stdio-bridge.ps1') -Destination $stage -Force
foreach ($d in @('runcode', 'guard', 'plugin')) {
    Copy-Item -LiteralPath (Join-Path $runtimeSrc $d) -Destination (Join-Path $stage $d) -Recurse -Force
}
$required = @('stdio-bridge.ps1', 'runcode\RunCode.pyt', 'guard\watch_pro_dialog.ps1', 'plugin\mcp_bridge.py')
$missing = @($required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $stage $_)) })
if ($missing.Count -gt 0) {
    Remove-Item -LiteralPath $stage -Recurse -Force
    Fail ('运行时不完整，安装中止（未改动现有运行时）：' + ($missing -join ', '))
    exit 1
}
$bakRt = $null
if (Test-Path -LiteralPath $runtimeDst) {
    $bakRt = $runtimeDst + '.bak-' + $stamp
    Move-Item -LiteralPath $runtimeDst -Destination $bakRt -Force
}
try {
    Move-Item -LiteralPath $stage -Destination $runtimeDst -Force
} catch {
    if ($bakRt) { Move-Item -LiteralPath $bakRt -Destination $runtimeDst -Force }
    Fail ('运行时替换失败，已回滚：' + $_.Exception.Message)
    exit 1
}
if ($bakRt) { Ok ('运行时已更新（旧版备份：' + (Split-Path -Leaf $bakRt) + '）') } else { Ok '运行时已部署' }
Ok ('运行时目录：' + $runtimeDst)

$marker = [ordered]@{
    package       = 'ArcGIS Pro MCP 优化版（客户版）'
    version       = '1.1.11'
    installed_at  = (Get-Date).ToString('s')
    addin_sha256  = $srcHash
    package_root  = $pkgRoot
}
$markerJson = $marker | ConvertTo-Json
[IO.File]::WriteAllText((Join-Path $runtimeDst 'installed.json'), $markerJson, (New-Object System.Text.UTF8Encoding($false)))
Ok '安装标记 installed.json 已写入'

# ---------- 7. 客户端配置 ----------
if (-not $SkipClients) {
    Step '配置 AI 客户端（Claude Desktop / Claude Code）'
    try {
        & (Join-Path $PSScriptRoot 'configure-claude.ps1') -Quiet
    } catch {
        Warn ('客户端自动配置未完成：' + $_.Exception.Message)
        Warn '可稍后单独运行 02-configure-claude.cmd'
    }
}

# ---------- 8. 收尾提示 ----------
Step '下一步'
Write-Host '   1) 打开 ArcGIS Pro。首次加载本 add-in 时如提示“加载项不受信任”，请选择“是 / 信任”。'
Write-Host '   2) 在 Pro 顶部找到 MCP 选项卡，确认状态为 running（HTTP：127.0.0.1:6520）。'
Write-Host '   3) 运行 01-check.cmd 做一次自检。'
Write-Host '   4) 在 Claude / Codex 中用自然语言驱动 Pro；详见 ..\docs\使用指南.md。'
Write-Host ''
Write-Host ('（可选）RunCode 工具箱：' + (Join-Path $runtimeDst 'runcode\RunCode.pyt'))
Write-Host '（可选）Python 桥启动方法与弹窗守护用法见 ..\docs\使用指南.md'
exit 0
