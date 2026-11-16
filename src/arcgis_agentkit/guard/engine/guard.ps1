#requires -Version 5.1
param(
    [string]$Token = '',
    [ValidateSet('op', 'manual')][string]$Scope = 'manual',
    [int]$TimeoutSeconds = 0,
    [int]$DurationSeconds = 0,
    [int]$PollMilliseconds = 0,
    [int]$TestSeconds = 30,
    [int]$AlertMinSeconds = 30,
    [switch]$AlertOnly,
    [switch]$SelfTest,
    [switch]$NotifyTest,
    [string]$TestText = '',
    [string]$ConfigPath = '',
    [string]$SessionRoot = '',
    [string]$LogPath = '',
    [string]$Reason = '',
    [switch]$NoEventHook
)

# ArcGIS Pro on-demand dialog guard (engine 2.0.1).
#
# Lifecycle: summoned with a token, arms within ~2 s (ready.json), watches for
# the *third-party code library* trust dialog while the caller's operation runs,
# settles on the caller's settle file and exits after a short grace period.
# Safety: only the trust dialog is ever clicked; overwrite/delete/remove
# confirmations are alerted, never clicked.
#
# Session protocol (all under -SessionRoot, default
# %LOCALAPPDATA%\ArcGIS-AgentKit\guard\sessions):
#   requests\<token>.json   summoning request (written before the mutex)
#   <token>\ready.json      engine armed
#   <token>\settle          caller finished the operation
#   <token>\result.json     engine summary, written on exit

$ErrorActionPreference = 'Stop'
$script:EngineVersion = '2.0.1'
$script:Token = $Token
$script:Events = New-Object System.Collections.Generic.List[object]
$script:DialogSeen = $false
$script:Resolved = $false
$script:AlertForm = $null
$script:AlertShownAt = [datetime]::MinValue
$script:LastBeep = [datetime]::MinValue
$script:GraceUntil = $null
$script:NoButtonSince = $null
$script:AlertOnlyMode = [bool]$AlertOnly

if ($TestSeconds -lt 30) { $TestSeconds = 30 }
if ($AlertMinSeconds -lt 30) { $AlertMinSeconds = 30 }

function ConvertTo-HashtableDeep($Object) {
    if ($null -eq $Object) { return $null }
    if ($Object -is [System.Collections.IDictionary]) {
        $result = @{}
        foreach ($key in $Object.Keys) { $result[$key] = ConvertTo-HashtableDeep $Object[$key] }
        return $result
    }
    if ($Object -is [System.Management.Automation.PSCustomObject]) {
        $result = @{}
        foreach ($property in $Object.PSObject.Properties) {
            $result[$property.Name] = ConvertTo-HashtableDeep $property.Value
        }
        return $result
    }
    if ($Object -is [System.Array]) {
        return @($Object | ForEach-Object { ConvertTo-HashtableDeep $_ })
    }
    return $Object
}

function Merge-Rules([hashtable]$Base, [hashtable]$Override) {
    foreach ($key in $Override.Keys) {
        if ($Base[$key] -is [hashtable] -and $Override[$key] -is [hashtable]) {
            Merge-Rules $Base[$key] $Override[$key]
        } else {
            $Base[$key] = $Override[$key]
        }
    }
}

$script:Rules = @{
    version = 2
    match = @{
        strong = @('第三方', '代码库', '第三方代码', 'third-party', 'third party', 'code library', 'code-library')
        weak = @('Python')
        affirm_buttons = @('是', '允许', 'Yes', 'Allow')
        tallow = @('允许', '执行', '信任', 'trust', 'allow', 'run')
        deny = @('覆盖', '删除', '移除', '卸载', '清空', '格式化', 'overwrite', 'delete', 'remove', 'uninstall', 'erase', 'replace')
    }
    alert = @{ min_seconds = 30; sound = $true; position = 'top-right' }
    limits = @{
        arm_ready_ms = 2000
        settle_grace_s = 3
        op_timeout_s = 180
        manual_timeout_s = 600
        poll_active_ms = 400
        poll_grace_ms = 1500
    }
}

$script:RulesPath = ''
$rulesCandidate = if ($ConfigPath) { $ConfigPath } else { Join-Path $PSScriptRoot 'guard.schema.json' }
if ($rulesCandidate -and (Test-Path -LiteralPath $rulesCandidate)) {
    try {
        $loaded = ConvertTo-HashtableDeep (Get-Content -LiteralPath $rulesCandidate -Raw -Encoding UTF8 | ConvertFrom-Json)
        if ($loaded -is [hashtable]) {
            Merge-Rules $script:Rules $loaded
            $script:RulesPath = (Resolve-Path -LiteralPath $rulesCandidate).Path
        }
    } catch {
        # A broken override must not disable the guard; built-in rules stay.
    }
}
$script:AlertMinSeconds = [math]::Max(30, [int]$script:Rules['alert']['min_seconds'])
if ($script:AlertMinSeconds -lt $AlertMinSeconds) { $script:AlertMinSeconds = $AlertMinSeconds }

function Join-RegexTerms($Terms) {
    $escaped = @($Terms | ForEach-Object { [regex]::Escape([string]$_) })
    if ($escaped.Count -eq 0) { return '(?!)' }
    return '(' + ($escaped -join '|') + ')'
}

$script:StrongRegex = [regex]::new((Join-RegexTerms $script:Rules['match']['strong']), 'IgnoreCase')
$script:WeakRegex = [regex]::new((Join-RegexTerms $script:Rules['match']['weak']), 'IgnoreCase')
$script:DenyRegex = [regex]::new((Join-RegexTerms $script:Rules['match']['deny']), 'IgnoreCase')
$script:TallowRegex = [regex]::new((Join-RegexTerms $script:Rules['match']['tallow']), 'IgnoreCase')
$script:AffirmRegex = [regex]::new(
    '^(?:' + ((@($script:Rules['match']['affirm_buttons']) | ForEach-Object { [regex]::Escape([string]$_) }) -join '|') + ')(?:\s*[（(][^)）]*[)）])?$',
    'IgnoreCase')
$script:OkRegex = [regex]::new('^(?:确定|OK)(?:\s*[（(][^)）]*[)）])?$', 'IgnoreCase')

$script:SessionRoot = $SessionRoot
if (-not $script:SessionRoot) {
    $base = if ($env:LOCALAPPDATA) { $env:LOCALAPPDATA } else { Join-Path $env:USERPROFILE 'AppData\Local' }
    $script:SessionRoot = Join-Path $base 'ArcGIS-AgentKit\guard\sessions'
}
$script:RequestsDir = Join-Path $script:SessionRoot 'requests'
$null = New-Item -ItemType Directory -Force -Path $script:RequestsDir -ErrorAction SilentlyContinue

$stateRoot = Split-Path (Split-Path $script:SessionRoot -Parent) -Parent
$script:JsonlPath = Join-Path (Join-Path $stateRoot 'logs') 'guard.jsonl'
$null = New-Item -ItemType Directory -Force -Path (Split-Path $script:JsonlPath -Parent) -ErrorAction SilentlyContinue

if (-not $script:Token) {
    $script:Token = 'manual-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + ([guid]::NewGuid().ToString('N').Substring(0, 8))
}

if ($DurationSeconds -gt 0) { $TimeoutSeconds = $DurationSeconds }
if ($TimeoutSeconds -le 0) {
    $TimeoutSeconds = if ($Scope -eq 'op') { [int]$script:Rules['limits']['op_timeout_s'] } else { [int]$script:Rules['limits']['manual_timeout_s'] }
}
if ($TimeoutSeconds -lt 10) { $TimeoutSeconds = 10 }

$activePoll = if ($PollMilliseconds -gt 0) { $PollMilliseconds } else { [int]$script:Rules['limits']['poll_active_ms'] }
$gracePoll = if ($PollMilliseconds -gt 0) { [math]::Max(400, $PollMilliseconds) } else { [int]$script:Rules['limits']['poll_grace_ms'] }
$settleGraceSeconds = [int]$script:Rules['limits']['settle_grace_s']

function Write-Event([string]$Level, [string]$Event, [hashtable]$Fields) {
    $record = [ordered]@{
        ts = (Get-Date).ToString('o')
        level = $Level
        event = $Event
        module = 'guard'
        engine_version = $script:EngineVersion
        pid = $PID
        token = $script:Token
        scope = $Scope
    }
    if ($Fields) { foreach ($key in $Fields.Keys) { $record[$key] = $Fields[$key] } }
    $script:Events.Add($record)
    if ($script:Events.Count -gt 200) { $script:Events.RemoveAt(0) }
    try {
        [IO.File]::AppendAllText($script:JsonlPath,
            (($record | ConvertTo-Json -Depth 8 -Compress) + [Environment]::NewLine),
            (New-Object System.Text.UTF8Encoding($false)))
    } catch { }
    if ($LogPath) {
        try {
            $line = '{0} {1} {2} {3}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff'), $Level, $Event, ($Fields | ConvertTo-Json -Depth 5 -Compress)
            [IO.File]::AppendAllText($LogPath, $line + [Environment]::NewLine,
                (New-Object System.Text.UTF8Encoding($false)))
        } catch { }
    }
}

function Get-TextMatch([string]$Text) {
    $strong = $script:StrongRegex.IsMatch($Text)
    $weak = $script:WeakRegex.IsMatch($Text)
    $deny = $script:DenyRegex.IsMatch($Text)
    if ($deny) {
        return @{ action = 'alert'; reason = 'deny'; strong = $strong; weak = $weak; deny = $true }
    }
    if ($strong) {
        return @{ action = 'click'; reason = $(if ($weak) { 'strong+weak' } else { 'strong' }); strong = $true; weak = $weak; deny = $false }
    }
    if ($weak) {
        return @{ action = 'click'; reason = 'weak'; strong = $false; weak = $true; deny = $false }
    }
    return @{ action = 'none'; reason = 'no-context'; strong = $false; weak = $false; deny = $false }
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$AE = [System.Windows.Automation.AutomationElement]
$TreeScope = [System.Windows.Automation.TreeScope]
$ControlType = [System.Windows.Automation.ControlType]
$InvokePattern = [System.Windows.Automation.InvokePattern]

# Cross-thread wake-up flag for the UIA event handler (static so the event
# thread does not need a PowerShell runspace to touch script scope).
if (-not ('ArcAgentKitSignal' -as [type])) {
    Add-Type -TypeDefinition @'
namespace ArcAgentKit {
    public static class Signal {
        public static volatile bool NeedsScan;
    }
}
'@
}

$script:EventHookRegistered = $false
if (-not $NoEventHook) {
    try {
        $script:AutomationHandler = [System.Windows.Automation.AutomationEventHandler] {
            param($sender, $eventArgs)
            [ArcAgentKit.Signal]::NeedsScan = $true
        }
        [System.Windows.Automation.Automation]::AddAutomationEventHandler(
            [System.Windows.Automation.WindowPatternIdentifiers]::WindowOpenedEvent,
            [System.Windows.Automation.AutomationElement]::RootElement,
            [System.Windows.Automation.TreeScope]::Children,
            $script:AutomationHandler)
        $script:EventHookRegistered = $true
    } catch {
        $script:EventHookRegistered = $false
    }
}

Add-Type -Namespace ArcAgentKit -Name Native -MemberDefinition @'
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
[DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
[DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
'@

function Get-ProProcesses {
    @(Get-Process -Name ArcGISPro -ErrorAction SilentlyContinue)
}

function Get-ProIds {
    @(Get-ProProcesses | ForEach-Object { $_.Id })
}

function Bring-ProToFront([int[]]$ProIds, $DlgWindow) {
    try {
        $foreground = [ArcAgentKit.Native]::GetForegroundWindow()
        $foregroundPid = [uint32]0
        [void][ArcAgentKit.Native]::GetWindowThreadProcessId($foreground, [ref]$foregroundPid)
        if ($ProIds -contains [int]$foregroundPid) { return $true }
    } catch { }
    if ($DlgWindow) {
        try {
            $handle = [IntPtr][long]$DlgWindow.Current.NativeWindowHandle
            if ($handle -ne [IntPtr]::Zero) {
                [void][ArcAgentKit.Native]::ShowWindow($handle, 9)
                if ([ArcAgentKit.Native]::SetForegroundWindow($handle)) { return $true }
            }
        } catch { }
    }
    try {
        $shell = New-Object -ComObject WScript.Shell
        foreach ($id in $ProIds) { if ($shell.AppActivate($id)) { return $true } }
    } catch { }
    return $false
}

function Get-ProTopWindows([int[]]$ProIds) {
    $root = $AE::RootElement
    $result = @()
    foreach ($proId in $ProIds) {
        $condition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ProcessIdProperty, $proId
        try {
            $windows = $root.FindAll($TreeScope::Children, $condition)
            foreach ($window in $windows) { $result += $window }
        } catch { }
    }
    return $result
}

function Get-WindowButtons($Window) {
    try {
        $condition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ControlTypeProperty, $ControlType::Button
        return @($Window.FindAll($TreeScope::Descendants, $condition))
    } catch {
        return @()
    }
}

function Get-WindowText($Window) {
    $parts = New-Object System.Collections.Generic.List[string]
    try { if ($Window.Current.Name) { $parts.Add([string]$Window.Current.Name) } } catch { }
    foreach ($type in @($ControlType::Text, $ControlType::Edit)) {
        try {
            $condition = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ControlTypeProperty, $type
            $items = $Window.FindAll($TreeScope::Descendants, $condition)
            foreach ($item in $items) {
                try { if ($item.Current.Name) { $parts.Add([string]$item.Current.Name) } } catch { }
            }
        } catch { }
    }
    $text = ($parts -join ' ')
    if ($text.Length -gt 4000) { $text = $text.Substring(0, 4000) }
    return $text
}

function Test-AffirmButton([string]$Name) {
    if (-not $Name) { return $false }
    return $script:AffirmRegex.IsMatch($Name.Replace('&', '').Trim())
}

function Test-OkButton([string]$Name) {
    if (-not $Name) { return $false }
    return $script:OkRegex.IsMatch($Name.Replace('&', '').Trim())
}

function Find-TrustCandidate([int[]]$ProIds) {
    $windows = @(Get-ProTopWindows $ProIds)
    foreach ($window in $windows) {
        $buttons = @(Get-WindowButtons $window)
        if ($buttons.Count -eq 0) { continue }

        $affirm = $null; $affirmName = ''
        $okButton = $null; $okName = ''
        $names = New-Object System.Collections.Generic.List[string]
        foreach ($button in $buttons) {
            $name = ''
            try { $name = ([string]$button.Current.Name).Trim() } catch { }
            if ($name) { $names.Add($name) }
            if (-not $affirm -and (Test-AffirmButton $name)) { $affirm = $button; $affirmName = $name }
            elseif (-not $okButton -and (Test-OkButton $name)) { $okButton = $button; $okName = $name }
        }

        $text = Get-WindowText $window
        if (-not $text) { continue }
        $match = Get-TextMatch $text
        if ($match.action -eq 'none') { continue }

        $windowName = ''
        try { $windowName = [string]$window.Current.Name } catch { }
        $base = @{
            Window = $window
            WinName = $windowName
            Text = $text
            Buttons = $names
            Action = $match.action
            Reason = $match.reason
            Strong = $match.strong
            Weak = $match.weak
            Deny = $match.deny
        }

        if ($match.deny) {
            $base.Action = 'alert'
            $base.Button = $null
            $base.ButtonName = ''
            return $base
        }
        if ($affirm) {
            $base.Action = 'click'
            $base.Button = $affirm
            $base.ButtonName = $affirmName
            return $base
        }
        if ($match.strong -and $okButton) {
            $base.Action = 'click'
            $base.Button = $okButton
            $base.ButtonName = $okName
            return $base
        }
        if ($match.strong) {
            $base.Action = 'alert'
            $base.Button = $null
            $base.ButtonName = ''
            return $base
        }
        # A weak-only match without an affirmative button is not our dialog.
    }
    return $null
}

function Invoke-Button($Button) {
    try {
        $invoke = $Button.GetCurrentPattern($InvokePattern::Pattern)
        $invoke.Invoke()
        return $true
    } catch { }
    try {
        $legacy = $Button.GetCurrentPattern([System.Windows.Automation.LegacyIAccessiblePattern]::Pattern)
        $legacy.DoDefaultAction()
        return $true
    } catch { }
    return $false
}

function Show-Alert([string]$Detail) {
    if ($script:AlertForm) { return }
    try {
        $text = 'ArcGIS Pro 正在等待确认：' + [Environment]::NewLine +
                '“是否允许第三方代码库 Python 命令执行”' + [Environment]::NewLine +
                '请切换到 ArcGIS Pro，人工确认。'
        if ($Detail) { $text = $text + [Environment]::NewLine + $Detail }

        $form = [System.Windows.Forms.Form]::new()
        $form.Text = 'ArcGIS Pro 需要你确认'
        $form.FormBorderStyle = 'FixedToolWindow'
        $area = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
        $form.StartPosition = 'Manual'
        $form.ClientSize = [System.Drawing.Size]::new(600, 190)
        $form.Location = [System.Drawing.Point]::new(
            [int]($area.Right - $form.Width - 24), [int]($area.Top + 24))
        $form.TopMost = $true
        $form.ShowInTaskbar = $true
        $form.BackColor = [System.Drawing.Color]::FromArgb(255, 246, 213)

        $label = [System.Windows.Forms.Label]::new()
        $label.Dock = 'Fill'
        $label.Padding = [System.Windows.Forms.Padding]::new(14)
        $label.TextAlign = 'MiddleCenter'
        $label.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 11, [System.Drawing.FontStyle]::Bold)
        $label.ForeColor = [System.Drawing.Color]::FromArgb(140, 32, 32)
        $label.Text = $text

        $form.Controls.Add($label)
        $form.Show()
        $script:AlertForm = $form
        $script:AlertShownAt = Get-Date
        [System.Media.SystemSounds]::Exclamation.Play()
        $script:LastBeep = Get-Date
        Write-Event 'warn' 'alert_shown' @{ detail = $Detail }
    } catch {
        Write-Event 'error' 'alert_failed' @{ error = $_.Exception.Message }
    }
}

function Close-Alert([switch]$Force) {
    if (-not $script:AlertForm) { return }
    if (-not $Force) {
        $age = ((Get-Date) - $script:AlertShownAt).TotalSeconds
        if ($age -lt $script:AlertMinSeconds) { return }
    }
    try { $script:AlertForm.Close(); $script:AlertForm.Dispose() } catch { }
    $script:AlertForm = $null
    Write-Event 'info' 'alert_closed' @{ }
}

function Keep-AlertOnTop {
    if (-not $script:AlertForm) { return }
    try {
        [System.Windows.Forms.Application]::DoEvents()
        $handle = $script:AlertForm.Handle
        if ($handle -ne [IntPtr]::Zero) {
            [void][ArcAgentKit.Native]::SetWindowPos($handle, [IntPtr]::new(-1), 0, 0, 0, 0, 0x0013)
            $script:AlertForm.BringToFront()
        }
        $now = Get-Date
        if (($now - $script:LastBeep).TotalSeconds -ge 10 -and -not $script:Resolved) {
            [System.Media.SystemSounds]::Exclamation.Play()
            $script:LastBeep = $now
            [void](Bring-ProToFront (Get-ProIds) $null)
        }
    } catch { }
}

# ---- test modes ------------------------------------------------------------

if ($NotifyTest) {
    Show-Alert ("（弹窗可见性测试，{0} 秒后自动关闭）" -f $TestSeconds)
    $until = (Get-Date).AddSeconds($TestSeconds)
    while ((Get-Date) -lt $until) {
        [System.Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 100
    }
    Close-Alert -Force
    exit 0
}

if ($SelfTest) {
    if ($TestText) {
        $match = Get-TextMatch $TestText
        $result = [ordered]@{
            test = 'rules'
            action = $match.action
            reason = $match.reason
            strong = $match.strong
            weak = $match.weak
            deny = $match.deny
        }
        $result | ConvertTo-Json -Compress
        exit 0
    }
    $pros = Get-ProProcesses
    if (-not $pros) { 'ArcGIS Pro not running'; exit 0 }
    $ids = @($pros | ForEach-Object { $_.Id })
    'ArcGIS Pro PID: ' + ($ids -join ', ')
    foreach ($window in @(Get-ProTopWindows $ids)) {
        $name = ''; $class = ''; $type = ''; $modal = $false
        try {
            $name = [string]$window.Current.Name
            $class = [string]$window.Current.ClassName
            $type = $window.Current.ControlType.ProgrammaticName
            try { $modal = [bool]$window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Current.IsModal } catch { }
        } catch { }
        '--- window: name="{0}" class="{1}" type="{2}" modal={3}' -f $name, $class, $type, $modal
        $buttons = @(Get-WindowButtons $window)
        '    buttons: ' + $buttons.Count
        $index = 0
        foreach ($button in $buttons) {
            $index++
            if ($index -gt 30) { '    ... (truncated)'; break }
            $buttonName = ''
            try { $buttonName = [string]$button.Current.Name } catch { }
            '      [{0}] {1}' -f $index, $buttonName
        }
    }
    $candidate = Find-TrustCandidate $ids
    if ($candidate) {
        $excerpt = ($candidate.Text -replace '\s+', ' ')
        if ($excerpt.Length -gt 300) { $excerpt = $excerpt.Substring(0, 300) }
        '>>> CANDIDATE: win="{0}" button="{1}" action="{2}" reason="{3}" text="{4}"' -f $candidate.WinName, $candidate.ButtonName, $candidate.Action, $candidate.Reason, $excerpt
        '>>> buttons: ' + ($candidate.Buttons -join ' | ')
    } else {
        '>>> no trust dialog currently detected'
    }
    exit 0
}

# ---- session protocol ------------------------------------------------------

$mutex = $null
$ownsMutex = $false
try { $mutex = [System.Threading.Mutex]::new($false, 'Local\ArcGISProDialogWatch_v2') } catch { }
if ($mutex) {
    try { $ownsMutex = $mutex.WaitOne(0) }
    catch [System.Threading.AbandonedMutexException] { $ownsMutex = $true }
}

$request = [ordered]@{
    token = $script:Token
    scope = $Scope
    timeout_s = $TimeoutSeconds
    alert_only = [bool]$AlertOnly
    created = (Get-Date).ToString('o')
    reason = $Reason
    pid = $PID
}
$requestPath = Join-Path $script:RequestsDir ($script:Token + '.json')
try {
    [IO.File]::WriteAllText($requestPath, ($request | ConvertTo-Json -Compress),
        (New-Object System.Text.UTF8Encoding($false)))
} catch { }

Write-Event 'info' 'engine_start' @{
    owns_mutex = $ownsMutex
    session_root = $script:SessionRoot
    rules = $script:RulesPath
    reason = $Reason
}

if (-not $ownsMutex) {
    # Another engine owns the watch. It will arm this token; hand over and exit.
    $readyPath = Join-Path (Join-Path $script:SessionRoot $script:Token) 'ready.json'
    $until = (Get-Date).AddSeconds(2.5)
    while ((Get-Date) -lt $until -and -not (Test-Path -LiteralPath $readyPath)) {
        Start-Sleep -Milliseconds 100
    }
    Write-Event 'info' 'secondary_exit' @{ ready = (Test-Path -LiteralPath $readyPath) }
    if ($mutex) { $mutex.Dispose() }
    exit 0
}

$script:Tokens = @{}

function Register-Token($Request) {
    $token = [string]$Request.token
    if (-not $token -or $script:Tokens.ContainsKey($token)) { return }
    $sessionDir = Join-Path $script:SessionRoot $token
    $null = New-Item -ItemType Directory -Force -Path $sessionDir -ErrorAction SilentlyContinue
    $timeout = [int]$Request.timeout_s
    if ($timeout -le 0) { $timeout = $TimeoutSeconds }
    $entry = [pscustomobject]@{
        Token = $token
        Scope = [string]$Request.scope
        AlertOnly = [bool]$Request.alert_only
        SessionDir = $sessionDir
        CreatedAt = Get-Date
        Deadline = (Get-Date).AddSeconds($timeout)
        Settled = $false
        Expired = $false
        Detected = $false
        Clicked = $false
        Alerted = $false
        Resolved = $false
        ExitReason = ''
    }
    $script:Tokens[$token] = $entry
    $ready = [ordered]@{
        pid = $PID
        token = $token
        scope = $entry.Scope
        armed_at = (Get-Date).ToString('o')
        engine_version = $script:EngineVersion
        pro_pids = @(Get-ProIds)
        rules = $script:RulesPath
    }
    try {
        [IO.File]::WriteAllText((Join-Path $sessionDir 'ready.json'),
            ($ready | ConvertTo-Json -Compress),
            (New-Object System.Text.UTF8Encoding($false)))
    } catch { }
    Write-Event 'info' 'token_armed' @{ token = $token; scope = $entry.Scope; timeout_s = $timeout }
    try { Remove-Item -LiteralPath (Join-Path $script:RequestsDir ($token + '.json')) -Force -ErrorAction SilentlyContinue } catch { }
}

function Mark-Tokens([string]$Field) {
    foreach ($entry in @($script:Tokens.Values)) {
        if (-not $entry.Settled -or $Field -eq 'Detected') { $entry.$Field = $true }
    }
}

function Complete-Tokens([string]$Reason) {
    foreach ($entry in @($script:Tokens.Values)) {
        $result = [ordered]@{
            token = $entry.Token
            engine_pid = $PID
            engine_version = $script:EngineVersion
            scope = $entry.Scope
            detected = [bool]$entry.Detected
            clicked = [bool]$entry.Clicked
            alerted = [bool]$entry.Alerted
            resolved = [bool]$entry.Resolved
            settled = [bool]$entry.Settled
            expired = [bool]$entry.Expired
            exit_reason = $(if ($entry.ExitReason) { $entry.ExitReason } else { $Reason })
            at = (Get-Date).ToString('o')
            events = @($script:Events | Select-Object -Last 50)
        }
        try {
            [IO.File]::WriteAllText((Join-Path $entry.SessionDir 'result.json'),
                ($result | ConvertTo-Json -Depth 8 -Compress),
                (New-Object System.Text.UTF8Encoding($false)))
        } catch { }
        Write-Event 'info' 'token_result' @{ token = $entry.Token; exit_reason = $result.exit_reason }
    }
}

$script:ExitReason = 'unknown'
$script:ProIds = @()
try {
    Register-Token ([pscustomobject]@{ token = $script:Token; scope = $Scope; timeout_s = $TimeoutSeconds; alert_only = [bool]$AlertOnly })
    $lastScan = [datetime]::MinValue

    while ($true) {
        # 1) accept new tokens (summoned while this engine already owns the watch)
        foreach ($file in @(Get-ChildItem -LiteralPath $script:RequestsDir -Filter '*.json' -ErrorAction SilentlyContinue)) {
            try {
                $incoming = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
                Register-Token $incoming
            } catch {
                try { Remove-Item -LiteralPath $file.FullName -Force -ErrorAction SilentlyContinue } catch { }
            }
        }

        # 2) settle / expiry
        foreach ($entry in @($script:Tokens.Values)) {
            if (-not $entry.Settled -and (Test-Path -LiteralPath (Join-Path $entry.SessionDir 'settle'))) {
                $entry.Settled = $true
                Write-Event 'info' 'token_settled' @{ token = $entry.Token }
            }
            if (-not $entry.Settled -and (Get-Date) -ge $entry.Deadline) {
                $entry.Settled = $true
                $entry.Expired = $true
                $entry.ExitReason = 'timeout'
                Write-Event 'warn' 'token_timeout' @{ token = $entry.Token }
            }
        }

        # 3) Pro must still exist
        $script:ProIds = @(Get-ProIds)
        if ($script:ProIds.Count -eq 0) {
            $script:ExitReason = 'pro-exit'
            break
        }

        # 4) exit when every token has settled and the grace period is quiet
        $activeTokens = @($script:Tokens.Values | Where-Object { -not $_.Settled })
        if ($activeTokens.Count -eq 0) {
            if (-not $script:GraceUntil) {
                $script:GraceUntil = (Get-Date).AddSeconds($settleGraceSeconds)
                Write-Event 'info' 'draining' @{ grace_s = $settleGraceSeconds }
            }
        } else {
            $script:GraceUntil = $null
        }

        # 5) scan (event-first when available, poll as the safety net)
        $interval = if ($script:GraceUntil) { $gracePoll } else { $activePoll }
        $needScan = [ArcAgentKit.Signal]::NeedsScan -or (((Get-Date) - $lastScan).TotalMilliseconds -ge $interval)
        $candidate = $null
        if ($needScan) {
            [ArcAgentKit.Signal]::NeedsScan = $false
            $lastScan = Get-Date
            try { $candidate = Find-TrustCandidate $script:ProIds }
            catch { Write-Event 'error' 'scan_failed' @{ error = $_.Exception.Message } }
        }

        if ($candidate) {
            $script:DialogSeen = $true
            Mark-Tokens 'Detected'
            $excerpt = ($candidate.Text -replace '\s+', ' ')
            if ($excerpt.Length -gt 400) { $excerpt = $excerpt.Substring(0, 400) }
            Write-Event 'warn' 'dialog_detected' @{
                window = $candidate.WinName
                text = $excerpt
                buttons = @($candidate.Buttons)
                action = $candidate.Action
                reason = $candidate.Reason
                strong = $candidate.Strong
                weak = $candidate.Weak
                deny = $candidate.Deny
            }
            [void](Bring-ProToFront $script:ProIds $candidate.Window)

            # A trust dialog appears before its buttons do: the UIA tree is
            # populated a moment later. In a live 2.0 test the first scan saw
            # the text but no 是/否 buttons, and alerting immediately produced a
            # false 30-second alarm even though the auto-click succeeded a
            # second later. Give the tree 2.5 s before treating it as a failure.
            $strongWithoutButton = (
                $candidate.Action -eq 'alert' -and
                -not $candidate.Deny -and
                -not $candidate.Button -and
                -not $script:AlertOnlyMode -and
                $candidate.Strong)

            if ($strongWithoutButton) {
                if (-not $script:NoButtonSince) {
                    $script:NoButtonSince = Get-Date
                    Write-Event 'info' 'dialog_button_wait' @{
                        window = $candidate.WinName
                        wait_s = 2.5
                    }
                }
                if (((Get-Date) - $script:NoButtonSince).TotalSeconds -ge 2.5) {
                    Mark-Tokens 'Alerted'
                    Show-Alert '检测到信任确认框，但未找到可安全点击的「是 / 允许」按钮。'
                }
            } elseif ($candidate.Action -eq 'alert' -or $candidate.Deny) {
                $script:NoButtonSince = $null
                Mark-Tokens 'Alerted'
                if ($candidate.Deny) {
                    Show-Alert '检测到可能覆盖 / 删除类的确认框，守卫不会点击，请人工确认。'
                } else {
                    Show-Alert '检测到信任确认框，但未找到可安全点击的「是 / 允许」按钮。'
                }
            } elseif ($script:AlertOnlyMode) {
                $script:NoButtonSince = $null
                Mark-Tokens 'Alerted'
                Show-Alert '（AlertOnly 模式：需要你手动点击）'
            } elseif (-not $candidate.Button) {
                $script:NoButtonSince = $null
                Mark-Tokens 'Alerted'
                Show-Alert '未找到可点击的按钮，需要你手动点击。'
            } else {
                $script:NoButtonSince = $null
                $clicked = Invoke-Button $candidate.Button
                Write-Event 'warn' 'click_attempt' @{ button = $candidate.ButtonName; ok = $clicked; reason = $candidate.Reason }
                if ($clicked) {
                    Start-Sleep -Milliseconds 450
                    $still = $null
                    try { $still = Find-TrustCandidate $script:ProIds } catch { }
                    if (-not $still) {
                        Mark-Tokens 'Clicked'
                        Mark-Tokens 'Resolved'
                        $script:Resolved = $true
                        Write-Event 'info' 'dialog_resolved' @{ method = 'auto-click'; reason = $candidate.Reason }
                        Close-Alert
                    } else {
                        Mark-Tokens 'Clicked'
                        Mark-Tokens 'Alerted'
                        Show-Alert '自动点击未生效，需要你手动点击。'
                    }
                } else {
                    Mark-Tokens 'Alerted'
                    Show-Alert '自动点击失败，需要你手动点击。'
                }
            }
        } else {
            $script:NoButtonSince = $null
            if ($script:DialogSeen -and -not $script:Resolved) {
                $script:Resolved = $true
                Mark-Tokens 'Resolved'
                Write-Event 'info' 'dialog_resolved' @{ method = 'external' }
            }
            Close-Alert
        }

        Keep-AlertOnTop

        if ($script:GraceUntil -and (Get-Date) -ge $script:GraceUntil) {
            $script:ExitReason = 'settled'
            break
        }

        # Sleep in small slices so UIA events wake us promptly.
        $sliceUntil = (Get-Date).AddMilliseconds([math]::Min(150, $interval))
        while ((Get-Date) -lt $sliceUntil) {
            if ([ArcAgentKit.Signal]::NeedsScan) { break }
            [System.Windows.Forms.Application]::DoEvents()
            Start-Sleep -Milliseconds 50
        }
    }
} finally {
    # Keep the alert visible for at least AlertMinSeconds even when exiting.
    if ($script:AlertForm) {
        try {
            $remaining = $script:AlertMinSeconds - ((Get-Date) - $script:AlertShownAt).TotalSeconds
            if ($remaining -gt 0) {
                $until = (Get-Date).AddSeconds($remaining)
                while ((Get-Date) -lt $until) {
                    [System.Windows.Forms.Application]::DoEvents()
                    Start-Sleep -Milliseconds 100
                }
            }
        } catch { }
    }
    Close-Alert -Force
    try { Complete-Tokens $script:ExitReason } catch { }
    Write-Event 'info' 'engine_end' @{ exit_reason = $script:ExitReason }
    if ($ownsMutex -and $mutex) { try { $mutex.ReleaseMutex() } catch { } }
    if ($mutex) { $mutex.Dispose() }
}

exit 0
