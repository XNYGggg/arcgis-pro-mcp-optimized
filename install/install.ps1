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

# Directory moves must be deterministic: PowerShell Move-Item happily moves a
# folder INTO an existing destination folder, which turned a failed upgrade
# into nested directories. .NET Move fails outright when the target exists.
function Move-Dir([string]$Source, [string]$Destination) {
    if (-not (Test-Path -LiteralPath $Source)) { throw ('missing source: ' + $Source) }
    if (Test-Path -LiteralPath $Destination) { throw ('destination already exists: ' + $Destination) }
    [System.IO.Directory]::Move($Source, $Destination)
}

$pkgRoot    = Split-Path -Parent $PSScriptRoot
$addinSrc   = Join-Path $PSScriptRoot 'ArcGISProMCP.esriAddinX'
$runtimeSrc = Join-Path $PSScriptRoot 'runtime'
$addinsDir  = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'ArcGIS\AddIns\ArcGISPro'
$runtimeDst = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP'
$stamp      = Get-Date -Format 'yyyyMMdd-HHmmss'

Write-Host 'ArcGIS Pro MCP 优化版 · 客户版 Customer 1.1.13（1.14 增强版 + 模板蒸馏）' -ForegroundColor White
Write-Host '组件：add-in + stdio 桥 + RunCode + 弹窗守护 + Python 桥 + 模板蒸馏（arcgis-distill）'

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
if ($ver.Major -gt 3 -or ($ver.Major -eq 3 -and $ver.Minor -gt 7)) {
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

# ---------- 5. 暂存运行时（先校验，后动 add-in） ----------
Step '暂存运行时组件'
$stage = Join-Path $env:LOCALAPPDATA ('ArcGIS Pro MCP.stage.' + $stamp)
if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null
Copy-Item -LiteralPath (Join-Path $runtimeSrc 'stdio-bridge.ps1') -Destination $stage -Force
foreach ($d in @('runcode', 'guard', 'plugin', 'distill', 'library')) {
    Copy-Item -LiteralPath (Join-Path $runtimeSrc $d) -Destination (Join-Path $stage $d) -Recurse -Force
}
$required = @('stdio-bridge.ps1', 'runcode\RunCode.pyt', 'guard\watch_pro_dialog.ps1', 'plugin\mcp_bridge.py', 'distill\run-distill.ps1', 'distill\arcgis_agentkit\distill\cli.py')
$missing = @($required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $stage $_)) })
if ($missing.Count -gt 0) {
    Remove-Item -LiteralPath $stage -Recurse -Force
    Fail ('运行时不完整，安装中止（未改动现有 add-in 与运行时）：' + ($missing -join ', '))
    exit 1
}
# 模板蒸馏库是用户数据：升级时优先保留旧库（含已蒸馏模板），
# 新包只提供空的 schema/index 种子。若当前运行时没有库（例如上一次回滚
# 把旧库留在了某次备份里），退回最近一次备份中的库。
$oldLibrary = Join-Path $runtimeDst 'library'
if (-not (Test-Path -LiteralPath $oldLibrary)) {
    $recentBakLibrary = Get-ChildItem -LiteralPath $env:LOCALAPPDATA -Directory -Filter 'ArcGIS Pro MCP.bak-*' -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object { Join-Path $_.FullName 'library' } |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1
    if ($recentBakLibrary) {
        $oldLibrary = $recentBakLibrary
        Ok ('未发现现役库，改用最近备份中的模板库：' + $recentBakLibrary)
    }
}
if (Test-Path -LiteralPath $oldLibrary) {
    $stagedLibrary = Join-Path $stage 'library'
    if (Test-Path -LiteralPath $stagedLibrary) { Remove-Item -LiteralPath $stagedLibrary -Recurse -Force }
    Copy-Item -LiteralPath $oldLibrary -Destination $stagedLibrary -Recurse -Force
    Ok '已保留旧模板库（library）'
}
Ok ('运行时已暂存并通过完整性校验：' + $stage)

# ---------- 6. 安装 add-in（原子替换；失败则清理暂存并中止） ----------
Step '安装 add-in（当前用户，不需要管理员）'
if (-not (Test-Path -LiteralPath $addinsDir)) {
    New-Item -ItemType Directory -Force -Path $addinsDir | Out-Null
}
$addinDst = Join-Path $addinsDir 'ArcGISProMCP.esriAddinX'
$srcHash = (Get-FileHash -LiteralPath $addinSrc -Algorithm SHA256).Hash
$needCopy = $true
$bakAddin = $null
$addinChanged = $false
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
        Remove-Item -LiteralPath $stage -Recurse -Force
        Fail 'add-in 复制校验失败，安装中止（未改动现有文件）'
        exit 1
    }
    if (Test-Path -LiteralPath $addinDst) {
        $bakAddin = $addinDst + '.bak-' + $stamp
        Move-Item -LiteralPath $addinDst -Destination $bakAddin -Force
    }
    try {
        Move-Item -LiteralPath $tmp -Destination $addinDst -Force
    } catch {
        if ($bakAddin) { Move-Item -LiteralPath $bakAddin -Destination $addinDst -Force }
        Remove-Item -LiteralPath $stage -Recurse -Force
        Fail ('add-in 替换失败，已回滚：' + $_.Exception.Message)
        exit 1
    }
    $addinChanged = $true
    if ($bakAddin) { Ok ('已更新（旧版备份：' + (Split-Path -Leaf $bakAddin) + '）') } else { Ok 'add-in 已安装' }
}
Ok ('SHA256 ' + $srcHash)

# ---------- 7. 提交运行时（任何一步失败都回滚，且绝不删除未动过的旧运行时） ----------
Step '提交运行时组件'
$bakRt = $null
if (Test-Path -LiteralPath $runtimeDst) {
    $bakRt = $runtimeDst + '.bak-' + $stamp
}
$movedToBak = $false
try {
    if ($bakRt) {
        Move-Dir $runtimeDst $bakRt
        $movedToBak = $true
    }
    Move-Dir $stage $runtimeDst
} catch {
    $reason = $_.Exception.Message
    $runtimeRestored = $false
    $addinRestored = $false
    $addinRemoved = $false
    $addinAbsent = $false
    if ($movedToBak) {
        # The old runtime (and the user's template library) is safe in $bakRt.
        # Remove a half-created destination, then put the old runtime back.
        try { if (Test-Path -LiteralPath $runtimeDst) { Remove-Item -LiteralPath $runtimeDst -Recurse -Force } }
        catch { $reason += ' | WARNING: cleanup failed: ' + $_.Exception.Message }
        try {
            if ($bakRt -and (Test-Path -LiteralPath $bakRt)) { Move-Dir $bakRt $runtimeDst; $runtimeRestored = $true }
            else { throw ('backup missing: ' + $bakRt) }
        } catch {
            $reason += ' | WARNING: runtime restore failed: ' + $_.Exception.Message
        }
    } else {
        # The very first move failed: $runtimeDst is still the untouched old
        # runtime (including the user's library), so leave it exactly as is.
    }
    if ($bakAddin) {
        try {
            if (Test-Path -LiteralPath $addinDst) { Move-Item -LiteralPath $addinDst -Destination ($addinDst + '.failed-' + $stamp) -Force }
            Move-Item -LiteralPath $bakAddin -Destination $addinDst -Force
            $addinRestored = $true
        } catch { $reason += ' | WARNING: add-in restore failed: ' + $_.Exception.Message }
    } elseif ($addinChanged) {
        # First install: no previous add-in to restore; take the new one out.
        try {
            if (Test-Path -LiteralPath $addinDst) {
                Move-Item -LiteralPath $addinDst -Destination ($addinDst + '.failed-' + $stamp) -Force
                $addinRemoved = $true
            } else {
                $addinAbsent = $true
            }
        } catch { $reason += ' | WARNING: add-in removal failed: ' + $_.Exception.Message }
    }
    try { if (Test-Path -LiteralPath $stage) { Remove-Item -LiteralPath $stage -Recurse -Force } }
    catch { $reason += ' | WARNING: staging cleanup failed: ' + $_.Exception.Message }

    if ($addinRestored) {
        $addinHint = '(旧版已从备份恢复原位)'
    } elseif (-not $addinChanged) {
        $addinHint = '(本机 add-in 未变更，无备份)'
    } elseif ($bakAddin) {
        if (Test-Path -LiteralPath $bakAddin) {
            $addinHint = $bakAddin + '（旧版备份仍在）'
        } else {
            $addinHint = '(升级；add-in 备份缺失：' + $bakAddin + ')'
        }
    } elseif ($addinRemoved) {
        $addinHint = '(首次安装；新 add-in 已移出为 .failed-' + $stamp + ')'
    } elseif ($addinAbsent) {
        $addinHint = '(首次安装；目标路径已无 add-in，无需移出)'
    } else {
        $addinHint = '(首次安装；新 add-in 移出失败，仍在原位)'
    }
    if (-not $bakRt) {
        $rtHint = '(无，首次安装)'
    } elseif ($runtimeRestored) {
        $rtHint = '(旧运行时已从备份恢复原位)'
    } elseif (-not $movedToBak) {
        $rtHint = '(未创建备份：旧运行时原地未动)'
    } elseif (Test-Path -LiteralPath $bakRt) {
        $rtHint = $bakRt + '（旧运行时备份仍在）'
    } else {
        $rtHint = '(备份缺失：' + $bakRt + ')'
    }
    $bakHint = 'add-in：' + $addinHint + '；运行时：' + $rtHint
    if ($reason -match 'WARNING') {
        Fail ('运行时替换失败，回滚未完全成功（' + $bakHint + '）：' + $reason)
    } else {
        Fail ('运行时替换失败，已回滚运行时与 add-in（' + $bakHint + '）：' + $reason)
    }
    exit 1
}
if ($bakRt) { Ok ('运行时已更新（旧版备份：' + (Split-Path -Leaf $bakRt) + '）') } else { Ok '运行时已部署' }
Ok ('运行时目录：' + $runtimeDst)

$marker = [ordered]@{
    package       = 'ArcGIS Pro MCP 优化版（客户版）'
    version       = '1.1.13'
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
