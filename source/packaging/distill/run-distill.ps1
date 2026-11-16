#requires -Version 5.1
<#
arcgis-distill 启动器（客户版，无需另装 Python/pip）
- 使用 ArcGIS Pro 自带 Python（含 Pillow）运行内置 arcgis_agentkit.distill 包
- 参数原样透传，例如：
    run-distill.ps1 list --json
    run-distill.ps1 import sample.png --name "研究区总览图" --json
    run-distill.ps1 preview <id> --out preview.png
- 使用简单脚本 + $args（不是 advanced script），避免 --out 被 PowerShell
  误绑定为公共参数 -OutVariable/-OutBuffer。
- 库默认落在 %LOCALAPPDATA%\ArcGIS Pro MCP\library；可用 ARCGIS_DISTILL_LIBRARY 覆盖。
#>
param()
$Rest = @($args)

$ErrorActionPreference = 'Stop'

function Find-ProPython {
    $candidates = @()
    if ($env:ProgramW6432) { $candidates += (Join-Path $env:ProgramW6432 'ArcGIS\Pro\bin\Python\envs\arcgispro-py3\python.exe') }
    if ($env:ProgramFiles) { $candidates += (Join-Path $env:ProgramFiles 'ArcGIS\Pro\bin\Python\envs\arcgispro-py3\python.exe') }
    if (${env:ProgramFiles(x86)}) { $candidates += (Join-Path ${env:ProgramFiles(x86)} 'ArcGIS\Pro\bin\Python\envs\arcgispro-py3\python.exe') }
    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate)) { return $candidate }
    }
    return $null
}

$python = Find-ProPython
if (-not $python) {
    [Console]::Error.WriteLine('未找到 ArcGIS Pro 自带 Python（arcgispro-py3）。请确认已安装 ArcGIS Pro 3.7+。')
    exit 3
}

$env:PYTHONPATH = $PSScriptRoot
$env:PYTHONUTF8 = '1'
$env:PYTHONIOENCODING = 'utf-8'
if (-not $env:ARCGIS_DISTILL_LIBRARY) {
    $env:ARCGIS_DISTILL_LIBRARY = Join-Path $env:LOCALAPPDATA 'ArcGIS Pro MCP\library'
}

& $python -m arcgis_agentkit.distill @Rest
exit $LASTEXITCODE
