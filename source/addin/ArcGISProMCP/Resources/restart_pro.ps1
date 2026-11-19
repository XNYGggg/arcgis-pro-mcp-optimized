<#
Restarts ArcGIS Pro after an MCP restart_pro call.

The add-in writes this file to %LOCALAPPDATA%\ArcGIS-MCP\runtime and starts
it detached: the MCP reply goes out first, then this script closes ArcGIS
Pro (answering "Don't Save" -- the add-in saved before asking), reopens the
project and waits until the command bridge answers again.

Every wait is bounded and every step leaves a timestamped line in
%LOCALAPPDATA%\ArcGIS-MCP\restart.log (UTF-8): the UI-automation probe that
answers the save prompt runs in a child PowerShell process and is abandoned
after $UiTimeoutSeconds, so a busy ArcGIS Pro can slow this helper down but
never wedge it. It never kills Pro: a forced kill would leave the project's
locks behind.

Runs hidden under Windows PowerShell 5.1.
#>
param(
    [string]$Project = "",
    [int]$ProPid = 0,
    [int]$DelaySeconds = 3,
    [int]$CloseTimeoutSeconds = 240,
    [int]$BridgeTimeoutSeconds = 240,
    [int]$UiTimeoutSeconds = 15
)

$ErrorActionPreference = "Stop"
$logRoot = Join-Path $env:LOCALAPPDATA "ArcGIS-MCP"
New-Item -ItemType Directory -Force $logRoot | Out-Null
$logPath = Join-Path $logRoot "restart.log"

function Write-Log([string]$message) {
    $line = "{0:s} {1}" -f (Get-Date), $message
    Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
}

function Get-Pro {
    if ($ProPid -gt 0) {
        return Get-Process -Id $ProPid -ErrorAction SilentlyContinue
    }
    return Get-Process ArcGISPro -ErrorAction SilentlyContinue
}

function Test-Bridge($port = 6510) {
    return $null -ne (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue)
}

# A plain TcpClient constructor can sit on the OS connect timeout; wait on the
# connect explicitly so one command cannot stall the helper.
function Open-Client($port = 6510, $connectTimeoutMs = 5000) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $iar = $client.BeginConnect("127.0.0.1", $port, $null, $null)
        if (-not $iar.AsyncWaitHandle.WaitOne($connectTimeoutMs)) {
            throw "connect to 127.0.0.1:$port timed out"
        }
        $client.EndConnect($iar)
        return $client
    } catch {
        $client.Close()
        throw
    }
}

function Send-Command($command, $timeoutMs = 120000) {
    $client = Open-Client 6510
    try {
        $stream = $client.GetStream()
        $payload = [Text.Encoding]::UTF8.GetBytes(
            "{`"id`":1,`"command`":`"$command`",`"params`":{}}`n")
        $stream.Write($payload, 0, $payload.Length)
        $stream.Flush()
        $client.ReceiveTimeout = $timeoutMs
        $buffer = New-Object byte[] 8192
        $read = $stream.Read($buffer, 0, $buffer.Length)
        return [Text.Encoding]::UTF8.GetString($buffer, 0, $read)
    } finally {
        $client.Close()
    }
}

# The save prompt is a WPF dialog inside the main window. Only the classic
# three-button shape (Save/Don't Save/Cancel, Chinese labels via the escapes
# below) is answered; any other dialog is logged and left alone, so a "No"
# button in an unrelated box can never be clicked.
#
# Every UI-automation call can block indefinitely while Pro is busy, so the
# whole probe runs in a child PowerShell process: after $UiTimeoutSeconds the
# job is stopped and the main script carries on. Buttons are matched with an
# optional "(N)" / "(&N)" suffix in either parenthesis width -- localized
# prompts use "否(N)" and "取消(C)", which an "(&N)-only" pattern misses.
function Invoke-SavePromptProbe([int]$processId) {
    $job = Start-Job -ArgumentList $processId -ScriptBlock {
        param($processId)
        try {
            Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes -ErrorAction Stop
            $root = [System.Windows.Automation.AutomationElement]::RootElement
            $byPid = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $processId)
            $window = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $byPid)
            if (-not $window) { return "none" }

            $isButton = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Button)
            $buttons = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $isButton)

            $suffix = '(\s*[（(](&?[A-Za-z])[）)])?$'
            $names = @()
            $noButton = $null
            foreach ($button in $buttons) {
                if ($button.Current.IsOffscreen -or -not $button.Current.IsEnabled) { continue }
                $name = $button.Current.Name
                if ($name) { $names += $name }
                if (-not $noButton -and
                    $name -match ('^(Don''t Save|Do not save|No|\u4e0d\u4fdd\u5b58|\u5426)' + $suffix)) {
                    $noButton = $button
                }
            }

            $hasSave = @($names | Where-Object {
                $_ -match ('^(Yes|Save|\u662f|\u4fdd\u5b58)' + $suffix) }).Count -gt 0
            $hasDiscard = @($names | Where-Object {
                $_ -match ('^(Don''t Save|Do not save|No|\u4e0d\u4fdd\u5b58|\u5426)' + $suffix) }).Count -gt 0
            $hasCancel = @($names | Where-Object {
                $_ -match ('^(Cancel|\u53d6\u6d88)' + $suffix) }).Count -gt 0

            if ($noButton -and $hasSave -and $hasDiscard -and $hasCancel) {
                $pattern = $noButton.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                $pattern.Invoke()
                return "clicked:" + $noButton.Current.Name
            }
            if ($names.Count -gt 0) {
                return "unexpected:" + ($names -join ",")
            }
            return "none"
        } catch {
            return "error:" + $_.Exception.Message
        }
    }

    $finished = Wait-Job $job -Timeout $UiTimeoutSeconds
    if (-not $finished) {
        Stop-Job $job -ErrorAction SilentlyContinue
        Remove-Job $job -Force -ErrorAction SilentlyContinue
        return "timeout"
    }
    try {
        $result = Receive-Job $job -ErrorAction SilentlyContinue | Select-Object -First 1
    } finally {
        Remove-Job $job -Force -ErrorAction SilentlyContinue
    }
    if ($null -eq $result) { return "none" }
    return [string]$result
}

Write-Log "restart requested: project='$Project' pid=$ProPid delay=$DelaySeconds"
if ($DelaySeconds -gt 0) { Start-Sleep -Seconds $DelaySeconds }

$pro = Get-Pro
if ($pro) {
    Write-Log "closing ArcGIS Pro (pid $($pro.Id))"
    $null = $pro.CloseMainWindow()

    $waited = 0
    $promptState = "pending"
    $uiaDisabled = $false
    $lastUnexpected = ""
    while ($waited -lt $CloseTimeoutSeconds) {
        $running = Get-Process -Id $pro.Id -ErrorAction SilentlyContinue
        if (-not $running) { break }
        Start-Sleep -Seconds 2
        $waited += 2

        if (-not $uiaDisabled -and $promptState -ne "clicked" -and $waited % 6 -eq 0) {
            $probe = Invoke-SavePromptProbe $running.Id
            switch -Wildcard ($probe) {
                "clicked:*" {
                    $promptState = "clicked"
                    Write-Log ("answered the save prompt: " + $probe.Substring(8))
                }
                "timeout" {
                    $uiaDisabled = $true
                    Write-Log ("save-prompt probe still blocked after ${UiTimeoutSeconds}s; not retrying")
                }
                "error:*" {
                    Write-Log ("save-prompt probe error: " + $probe.Substring(6))
                }
                "unexpected:*" {
                    $signature = $probe.Substring(11)
                    if ($signature -ne $lastUnexpected) {
                        $lastUnexpected = $signature
                        Write-Log ("unexpected dialog buttons: " + $signature + " -- not clicking")
                    }
                }
            }
        }

        if ($waited % 10 -eq 0) {
            $title = ""
            $responding = "n/a"
            try { $title = $running.MainWindowTitle } catch { }
            try { $responding = $running.Responding } catch { }
            Write-Log ("closing: waited=${waited}s responding=$responding title='$title'")
        }
    }

    if (Get-Process -Id $pro.Id -ErrorAction SilentlyContinue) {
        $title = ""
        try { $title = (Get-Process -Id $pro.Id).MainWindowTitle } catch { }
        Write-Log ("ArcGIS Pro did not close within ${CloseTimeoutSeconds}s (window: '$title'); giving up")
        exit 2
    }
    Write-Log "closed after ${waited}s"
} else {
    Write-Log "ArcGIS Pro was not running"
}

$exe = "C:\Program Files\ArcGIS\Pro\bin\ArcGISPro.exe"
if (-not (Test-Path $exe)) {
    $installDir = (Get-ItemProperty 'HKLM:\SOFTWARE\ESRI\ArcGISPro' -ErrorAction SilentlyContinue).InstallDir
    if (-not $installDir) { $installDir = Join-Path $env:ProgramW6432 "ArcGIS\Pro" }
    $exe = Join-Path $installDir "bin\ArcGISPro.exe"
}
if (-not (Test-Path $exe)) {
    Write-Log "ArcGIS Pro executable not found"
    exit 3
}

if ($Project -and (Test-Path $Project)) {
    Write-Log "starting ArcGIS Pro with $Project"
    Start-Process $exe -ArgumentList "`"$Project`""
} else {
    Write-Log "starting ArcGIS Pro without a project"
    Start-Process $exe
}

$waited = 0
$listening = $false
while ($waited -lt $BridgeTimeoutSeconds) {
    Start-Sleep -Seconds 5
    $waited += 5
    if (-not $listening -and (Test-Bridge)) {
        $listening = $true
        Write-Log "bridge listening after ${waited}s"
    }
    if ($listening) {
        try {
            if ((Send-Command "get_project_info" 20000) -match '"success"\s*:\s*true') {
                $ping = Send-Command "ping" 20000
                $count = 0
                if ($ping -match '"command_count"\s*:\s*(\d+)') { $count = [int]$Matches[1] }
                $newPid = 0
                if ($ping -match '"pid"\s*:\s*(\d+)') { $newPid = [int]$Matches[1] }
                if ($count -ge 112 -and ($ProPid -le 0 -or $newPid -ne $ProPid)) {
                    $ok = "{0:s} ok project='{1}' pid={2} commands={3}" -f `
                        (Get-Date), $Project, $newPid, $count
                    Add-Content -LiteralPath (Join-Path $logRoot "restart.ok") -Value $ok -Encoding UTF8
                    Write-Log "project open and verified after ${waited}s (commands=$count pid=$newPid)"
                    exit 0
                }
                Write-Log "bridge answered but verification failed (commands=$count pid=$newPid)"
                exit 5
            }
        } catch {
        }
    }
    if ($waited % 30 -eq 0) {
        Write-Log "waiting for the bridge: ${waited}s listening=$listening"
    }
}

Write-Log "bridge did not answer within ${BridgeTimeoutSeconds}s"
exit 1
