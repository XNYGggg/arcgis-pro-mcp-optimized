<#
.SYNOPSIS
    Allow the ArcGIS Pro MCP read/author tools in Claude Code without
    per-call prompts.

.DESCRIPTION
    Builds one allow rule per MCP tool (mcp__<server>__<tool>) from the live
    bridge or from tools.json, then merges them into a Claude Code settings
    file. Code-execution and destructive tools stay out of the allow list on
    purpose -- they keep asking every time.

    Nothing is written unless -Apply is passed: the default run only prints
    the plan. -Apply writes to the project-level
    <project>\.claude\settings.local.json unless -Scope user is given, backs
    the file up first, and is a no-op when the rules are unchanged. -Remove
    takes exactly this tool set back out and leaves every other rule alone.

.PARAMETER SettingsPath
    Explicit settings file to use; overrides -Scope/-ProjectDir.

.PARAMETER ToolsJson
    Tool schema file, used when the bridge cannot be asked. Defaults to the
    add-in resource in the source tree or a copy next to this script.

.PARAMETER Url
    MCP endpoint of a running add-in; tool names are read live from it.
    Pass "" to skip the bridge.

.PARAMETER ServerName
    MCP server name the rules target. Defaults to "arcgis".

.PARAMETER Scope
    project (default) writes <project>\.claude\settings.local.json;
    user writes %USERPROFILE%\.claude\settings.json.

.PARAMETER ProjectDir
    Project folder whose .claude\settings.local.json is written.
    Defaults to the current directory.

.PARAMETER Apply
    Actually write the settings file. Without it this is a dry run.

.PARAMETER Remove
    Remove the generated rules instead of adding them.

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts\install_claude_permissions.ps1

.EXAMPLE
    powershell -NoProfile -ExecutionPolicy Bypass -File scripts\install_claude_permissions.ps1 -Apply
#>
param(
    [string]$SettingsPath = "",
    [string]$ToolsJson = "",
    [string]$Url = "http://127.0.0.1:6520/mcp",
    [string]$ServerName = "arcgis",
    [ValidateSet("project", "user")]
    [string]$Scope = "project",
    [string]$ProjectDir = "",
    [switch]$Apply,
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

# Tools that run arbitrary code or delete things keep their per-call prompt.
$CONFIRM_ALWAYS = @(
    "run_python_toolbox_tool",
    "execute_arcpy_code",
    "restart_pro",
    "run_geoprocessing_tool",
    "delete_dataset",
    "delete_features",
    "delete_field",
    "delete_layout",
    "delete_layout_element",
    "delete_bookmark",
    "remove_map",
    "remove_layer",
    "remove_join",
    "truncate_table"
)

function Find-ToolsJson {
    $candidates = @()
    if ($ToolsJson) { $candidates += $ToolsJson }
    $candidates += (Join-Path $PSScriptRoot "..\addin\ArcGISProMCP\Resources\tools.json")
    $candidates += (Join-Path $PSScriptRoot "tools.json")
    foreach ($path in $candidates) {
        if ($path -and (Test-Path -LiteralPath $path)) {
            return (Resolve-Path -LiteralPath $path).Path
        }
    }
    throw "tools.json not found; pass -ToolsJson <path> or run with ArcGIS Pro open."
}

function Get-ToolNamesFromBridge([string]$address) {
    $headers = @{
        "Accept" = "application/json, text/event-stream"
        "MCP-Protocol-Version" = "2025-03-26"
    }
    $init = @{
        jsonrpc = "2.0"; id = 1; method = "initialize"
        params = @{
            protocolVersion = "2025-03-26"
            capabilities = @{}
            clientInfo = @{ name = "install-claude-permissions"; version = "1" }
        }
    } | ConvertTo-Json -Depth 8
    $null = Invoke-RestMethod -Uri $address -Method Post -Headers $headers `
        -ContentType "application/json" -Body $init -TimeoutSec 15
    try {
        $note = @{ jsonrpc = "2.0"; method = "notifications/initialized" } |
            ConvertTo-Json -Depth 4
        $null = Invoke-RestMethod -Uri $address -Method Post -Headers $headers `
            -ContentType "application/json" -Body $note -TimeoutSec 10
    } catch {
    }
    $list = @{ jsonrpc = "2.0"; id = 2; method = "tools/list"; params = @{} } |
        ConvertTo-Json -Depth 4
    $reply = Invoke-RestMethod -Uri $address -Method Post -Headers $headers `
        -ContentType "application/json" -Body $list -TimeoutSec 20
    return @($reply.result.tools | ForEach-Object { $_.name } |
        Where-Object { $_ })
}

if (-not $SettingsPath) {
    if ($Scope -eq "user") {
        $SettingsPath = Join-Path $env:USERPROFILE ".claude\settings.json"
    } else {
        if (-not $ProjectDir) { $ProjectDir = (Get-Location).Path }
        $SettingsPath = Join-Path $ProjectDir ".claude\settings.local.json"
    }
}

$toolNames = @()
$source = ""
if ($Url) {
    try {
        $toolNames = @(Get-ToolNamesFromBridge $Url)
        if ($toolNames.Count -gt 0) { $source = "live bridge $Url" }
    } catch {
        $toolNames = @()
    }
}
if ($toolNames.Count -eq 0) {
    $jsonPath = Find-ToolsJson
    $document = Get-Content -LiteralPath $jsonPath -Raw | ConvertFrom-Json
    $toolNames = @($document.tools | ForEach-Object { $_.name })
    $source = $jsonPath
}
if ($toolNames.Count -eq 0) {
    throw "No tool names found (bridge and tools.json both empty)."
}

$allRules = @($toolNames | ForEach-Object { "mcp__${ServerName}__" + $_ })
$confirmed = @($toolNames | Where-Object { $CONFIRM_ALWAYS -contains $_ })
$wanted = @($toolNames |
    Where-Object { $CONFIRM_ALWAYS -notcontains $_ } |
    ForEach-Object { "mcp__${ServerName}__" + $_ })

Write-Host "tool list : $source ($($toolNames.Count) tools)"
Write-Host "always ask: $($confirmed.Count) tools ($($confirmed -join ', '))"

$settings = $null
if (Test-Path -LiteralPath $SettingsPath) {
    $raw = Get-Content -LiteralPath $SettingsPath -Raw
    if ($raw.Trim()) { $settings = $raw | ConvertFrom-Json }
}
if (-not $settings) { $settings = [pscustomobject]@{} }

$permissions = $settings.permissions
if (-not $permissions) {
    $permissions = [pscustomobject]@{}
    $settings | Add-Member -NotePropertyName permissions -NotePropertyValue $permissions -Force
}
$current = @()
if ($permissions.PSObject.Properties.Name -contains "allow" -and $permissions.allow) {
    $current = @($permissions.allow)
}

if ($Remove) {
    $result = New-Object System.Collections.ArrayList
    foreach ($rule in $current) {
        if ($allRules -contains $rule) { continue }
        [void]$result.Add($rule)
    }
    $result = @($result)
} else {
    $result = New-Object System.Collections.ArrayList
    foreach ($rule in $current) { [void]$result.Add($rule) }
    foreach ($rule in $wanted) {
        if (-not $result.Contains($rule)) { [void]$result.Add($rule) }
    }
    $result = @($result)
}

$before = $current.Count
$after = $result.Count
$component = "mcp__" + $ServerName + "__"
$beforeCount = @($current | Where-Object { $_ -like ($component + "*") }).Count
$afterCount = @($result | Where-Object { $_ -like ($component + "*") }).Count
Write-Host "settings  : $SettingsPath"
Write-Host "rules $component*: $beforeCount -> $afterCount"

if (-not $Apply) {
    Write-Host "Preview only. Re-run with -Apply to write."
    exit 0
}
if ($beforeCount -eq $afterCount -and $before -eq $after) {
    Write-Host "No change; nothing written."
    exit 0
}

$permissions | Add-Member -NotePropertyName allow -NotePropertyValue $result -Force

if (Test-Path -LiteralPath $SettingsPath) {
    $backup = $SettingsPath + ".bak-" + (Get-Date -Format "yyyyMMdd-HHmmss")
    Copy-Item -LiteralPath $SettingsPath -Destination $backup
    Write-Host "backup    : $backup"
} else {
    $parent = Split-Path -Parent $SettingsPath
    if (-not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
}

$text = $settings | ConvertTo-Json -Depth 32
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($SettingsPath, $text, $utf8)
Write-Host ("written: allow rules " + $before + " -> " + $after)
