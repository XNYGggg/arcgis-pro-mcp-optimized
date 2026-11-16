param(
    [int]$DurationSeconds = 600,
    [int]$PollMilliseconds = 700,
    [int]$TestSeconds = 30,
    [int]$AlertMinSeconds = 30,
    [switch]$AlertOnly,
    [switch]$SelfTest,
    [switch]$NotifyTest
)

# ArcGIS Pro 第三方代码弹窗守护（用户要求，2026-11-09）
# 用途：当 ArcGIS Pro 弹出“是否允许第三方代码库 Python 命令执行”这类确认框时：
#   1) 自动点击「是/允许」；
#   2) 若自动点击失败，弹出全系统置顶提示窗并响铃，提醒用户切到 Pro 手动点击。
# 安全边界：只处理带“第三方/代码库/third-party/code library/Python”上下文的对话框，
#           绝不触碰其他“是/否”确认框（如覆盖输出、删除确认等）。
# 用户要求（2026-11-09）：所有短时间弹窗一律改为至少 30 秒——提醒窗显示后
#   至少保留 $AlertMinSeconds（默认且下限 30）秒；-NotifyTest 可见性测试默认 30 秒。
# 用法：
#   powershell -NoProfile -ExecutionPolicy Bypass -File watch_pro_dialog.ps1 [-DurationSeconds 600] [-AlertOnly] [-SelfTest] [-NotifyTest] [-TestSeconds 30] [-AlertMinSeconds 30]

if ($TestSeconds -lt 30) { $TestSeconds = 30 }
if ($AlertMinSeconds -lt 30) { $AlertMinSeconds = 30 }

$ErrorActionPreference = 'Stop'
$script:LogPath = Join-Path $PSScriptRoot 'watch_pro_dialog.log'

# 统一 macOS 红绿灯弹窗库（与本脚本同目录；缺失时回退到本脚本内置的黄底提醒窗）
$script:MacDialogLib = Join-Path $PSScriptRoot 'MacDialog.ps1'
$script:MacDialogLoaded = $false
if (Test-Path -LiteralPath $script:MacDialogLib) {
    try {
        . $script:MacDialogLib
        $script:MacDialogLoaded = [bool](Get-Command New-MacAlertWindow -ErrorAction SilentlyContinue)
    } catch { $script:MacDialogLoaded = $false }
}

function Write-Log([string]$Message) {
    try {
        $line = '{0} {1}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff'), $Message
        [IO.File]::AppendAllText($script:LogPath, $line + [Environment]::NewLine, (New-Object System.Text.UTF8Encoding($false)))
    } catch { }
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$AE = [System.Windows.Automation.AutomationElement]
$TreeScope = [System.Windows.Automation.TreeScope]
$ControlType = [System.Windows.Automation.ControlType]
$InvokePattern = [System.Windows.Automation.InvokePattern]

# Win32：把 Pro 窗口提到最前，把提醒窗钉在最顶层（用户要求：ENVI 导入等流程中弹窗必须是最上层）
Add-Type -Namespace ArcWpc -Name Native -MemberDefinition @'
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
[DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
[DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
'@

function Bring-ProToFront([int[]]$ProIds, $DlgWindow) {
    # 已经在前台时不做任何事，避免无谓抢焦点
    try {
        $fg = [ArcWpc.Native]::GetForegroundWindow()
        $fgPid = [uint32]0
        [void][ArcWpc.Native]::GetWindowThreadProcessId($fg, [ref]$fgPid)
        if ($ProIds -contains [int]$fgPid) { return $true }
    } catch { }

    if ($DlgWindow) {
        try {
            $h = [IntPtr][long]$DlgWindow.Current.NativeWindowHandle
            if ($h -ne [IntPtr]::Zero) {
                [void][ArcWpc.Native]::ShowWindow($h, 9)      # SW_RESTORE
                if ([ArcWpc.Native]::SetForegroundWindow($h)) { return $true }
            }
        } catch { }
    }
    try {
        $shell = New-Object -ComObject WScript.Shell
        foreach ($id in $ProIds) { if ($shell.AppActivate($id)) { return $true } }
    } catch { }
    return $false
}

function Get-ProProcesses {
    @(Get-Process -Name ArcGISPro -ErrorAction SilentlyContinue)
}

function Get-ProTopWindows([int[]]$ProIds) {
    $root = $AE::RootElement
    $result = @()
    foreach ($proId in $ProIds) {
        $cond = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ProcessIdProperty, $proId
        $wins = $root.FindAll($TreeScope::Children, $cond)
        foreach ($w in $wins) { $result += $w }
    }
    return $result
}

function Get-WindowArea([System.Windows.Automation.AutomationElement]$Win) {
    try {
        $r = $Win.Current.BoundingRectangle
        if ($null -eq $r) { return 0 }
        return [double]$r.Width * [double]$r.Height
    } catch { return 0 }
}

function Test-ModalWindow([System.Windows.Automation.AutomationElement]$Win) {
    try {
        $wp = $Win.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern)
        return [bool]$wp.Current.IsModal
    } catch { return $false }
}

function Get-WindowText([System.Windows.Automation.AutomationElement]$Win) {
    $parts = New-Object System.Collections.Generic.List[string]
    try { if ($Win.Current.Name) { $parts.Add([string]$Win.Current.Name) } } catch { }
    try {
        $textCond = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ControlTypeProperty, $ControlType::Text
        $texts = $Win.FindAll($TreeScope::Descendants, $textCond)
        foreach ($t in $texts) {
            try { if ($t.Current.Name) { $parts.Add([string]$t.Current.Name) } } catch { }
        }
    } catch { }
    ($parts -join ' ')
}

function Test-TrustDialogText([string]$AllText) {
    if ($AllText -match '第三方|代码库|third[- ]party|code\s*library') { return $true }
    if (($AllText -match 'Python') -and ($AllText -match '允许|执行|execution|allow|trust|信任')) { return $true }
    return $false
}

function Find-TrustDialog([int[]]$ProIds) {
    $windows = @(Get-ProTopWindows $ProIds)
    if ($windows.Count -eq 0) { return $null }

    foreach ($w in $windows) {
        $winName = ''
        try { $winName = [string]$w.Current.Name } catch { }

        # 先枚举按钮（主窗口平时也有几十个按钮，扫描很快）；只有出现“是/允许”按钮才继续做文字匹配
        $btnCond = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ControlTypeProperty, $ControlType::Button
        $btns = $null
        try { $btns = $w.FindAll($TreeScope::Descendants, $btnCond) } catch { continue }
        if ($btns.Count -eq 0) { continue }

        $affirm = $null; $affirmName = ''
        $okBtn = $null; $okName = ''
        $names = New-Object System.Collections.Generic.List[string]
        foreach ($b in $btns) {
            $n = ''
            try { $n = ([string]$b.Current.Name).Trim() } catch { }
            if ($n) { $names.Add($n) }
            if (-not $affirm -and $n -match '^(是|&是|Yes|&Yes|允许|Allow)(\s*[（(][^)）]*[)）])?$') { $affirm = $b; $affirmName = $n }
            elseif (-not $okBtn -and $n -match '^(确定|OK|&OK)(\s*[（(][^)）]*[)）])?$') { $okBtn = $b; $okName = $n }
        }
        if (-not $affirm -and -not $okBtn) { continue }

        $allText = Get-WindowText $w
        if (-not (Test-TrustDialogText $allText)) { continue }

        $strongContext = $allText -match '第三方|代码库|third[- ]party|code\s*library'
        if ($affirm) {
            return @{ Window = $w; Button = $affirm; ButtonName = $affirmName; Buttons = $names; Text = $allText; WinName = $winName }
        }
        if ($strongContext -and $okBtn) {
            return @{ Window = $w; Button = $okBtn; ButtonName = $okName; Buttons = $names; Text = $allText; WinName = $winName }
        }
        if ($strongContext) {
            return @{ Window = $w; Button = $null; ButtonName = ''; Buttons = $names; Text = $allText; WinName = $winName }
        }
    }
    return $null
}

function Invoke-Button([System.Windows.Automation.AutomationElement]$Btn) {
    try {
        $inv = $Btn.GetCurrentPattern($InvokePattern::Pattern)
        $inv.Invoke()
        return $true
    } catch { }
    try {
        $legacy = $Btn.GetCurrentPattern([System.Windows.Automation.LegacyIAccessiblePattern]::Pattern)
        $legacy.DoDefaultAction()
        return $true
    } catch { }
    return $false
}

$script:AlertForm = $null
$script:AlertShownAt = [datetime]::MinValue
$script:LastBeep = [datetime]::MinValue

function Show-Alert([string]$Detail) {
    if ($script:AlertForm) { return }
    try {
        $text = 'ArcGIS Pro 正在等待确认：' + [Environment]::NewLine +
                '“是否允许第三方代码库 Python 命令执行”' + [Environment]::NewLine +
                '请切换到 ArcGIS Pro，点击「是 / 允许」。'
        if ($Detail) { $text = $text + [Environment]::NewLine + $Detail }

        if ($script:MacDialogLoaded) {
            $form = New-MacAlertWindow -Title 'ArcGIS Pro 需要你点击确认' -Message $text -Width 600 -Height 220 -Position TopRight
            if ($form) {
                $form.Show()
                $script:AlertForm = $form
                $script:AlertShownAt = Get-Date
                [System.Media.SystemSounds]::Exclamation.Play()
                $script:LastBeep = Get-Date
                Write-Log ('alert shown (mac): ' + ($Detail -replace '\s+', ' '))
                return
            }
        }

        # 保底：原黄底提醒窗（只在全局库缺失/异常时使用）
        $form = [System.Windows.Forms.Form]::new()
        $form.Text = 'ArcGIS Pro 需要你点击确认'
        $form.FormBorderStyle = 'FixedToolWindow'
        # 屏幕右上角（避开 Pro 居中的确认框），并保持系统最顶层
        $area = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
        $form.StartPosition = 'Manual'
        $form.ClientSize = [System.Drawing.Size]::new(600, 170)
        $form.Location = [System.Drawing.Point]::new(
            [int]($area.Right - $form.Width - 24), [int]($area.Top + 24))
        $form.TopMost = $true
        $form.ShowInTaskbar = $true
        $form.BackColor = [System.Drawing.Color]::FromArgb(255, 246, 213)

        $label = [System.Windows.Forms.Label]::new()
        $label.Dock = 'Fill'
        $label.Padding = [System.Windows.Forms.Padding]::new(14)
        $label.TextAlign = 'MiddleCenter'
        $label.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 12, [System.Drawing.FontStyle]::Bold)
        $label.ForeColor = [System.Drawing.Color]::FromArgb(140, 32, 32)
        $label.Text = $text

        $form.Controls.Add($label)
        $form.Show()
        $script:AlertForm = $form
        $script:AlertShownAt = Get-Date
        [System.Media.SystemSounds]::Exclamation.Play()
        $script:LastBeep = Get-Date
        Write-Log ('alert shown: ' + ($Detail -replace '\s+', ' '))
    } catch {
        Write-Log ('alert show failed: ' + $_.Exception.Message)
    }
}

function Close-Alert([switch]$Force) {
    if ($script:AlertForm) {
        # 用户要求：弹窗至少显示 AlertMinSeconds（≥30）秒，不为 0 秒闪窗
        if (-not $Force) {
            $age = ((Get-Date) - $script:AlertShownAt).TotalSeconds
            if ($age -lt $AlertMinSeconds) { return }
        }
        try { $script:AlertForm.Close(); $script:AlertForm.Dispose() } catch { }
        $script:AlertForm = $null
        Write-Log 'alert closed'
    }
}

# ---- 测试模式 ----

if ($NotifyTest) {
    Show-Alert ("（这是一次弹窗可见性测试，{0} 秒后自动关闭）" -f $TestSeconds)
    $end = (Get-Date).AddSeconds($TestSeconds)
    while ((Get-Date) -lt $end) {
        [System.Windows.Forms.Application]::DoEvents()
        Start-Sleep -Milliseconds 100
    }
    Close-Alert -Force
    exit 0
}

if ($SelfTest) {
    $pros = Get-ProProcesses
    if (-not $pros) { 'ArcGIS Pro not running'; exit 0 }
    $ids = @($pros | ForEach-Object { $_.Id })
    'ArcGIS Pro PID: ' + ($ids -join ', ')
    $windows = @(Get-ProTopWindows $ids)
    foreach ($w in $windows) {
        $name = ''; $cls = ''; $ct = ''; $modal = $false
        try {
            $name = [string]$w.Current.Name
            $cls = [string]$w.Current.ClassName
            $ct = $w.Current.ControlType.ProgrammaticName
            $modal = Test-ModalWindow $w
        } catch { }
        '--- window: name="{0}" class="{1}" type="{2}" modal={3}' -f $name, $cls, $ct, $modal
        try {
            $btnCond = New-Object System.Windows.Automation.PropertyCondition -ArgumentList $AE::ControlTypeProperty, $ControlType::Button
            $btns = $w.FindAll($TreeScope::Descendants, $btnCond)
            '    buttons: ' + $btns.Count
            $i = 0
            foreach ($b in $btns) {
                $i++
                if ($i -gt 30) { '    ... (truncated)'; break }
                $n = ''; try { $n = [string]$b.Current.Name } catch { }
                '      [{0}] {1}' -f $i, $n
            }
        } catch {
            '    button scan failed: ' + $_.Exception.Message
        }
    }

    $dlg = Find-TrustDialog $ids
    if ($dlg) {
        $t = ($dlg.Text -replace '\s+', ' ')
        if ($t.Length -gt 300) { $t = $t.Substring(0, 300) }
        '>>> TRUST DIALOG FOUND: win="{0}" button="{1}" text="{2}"' -f $dlg.WinName, $dlg.ButtonName, $t
        '>>> buttons: ' + ($dlg.Buttons -join ' | ')
    } else {
        '>>> no trust dialog currently detected'
    }
    exit 0
}

# ---- 守护主循环 ----

$mutexName = 'Local\ArcGISProDialogWatch_v1'
$mutex = $null
try { $mutex = [System.Threading.Mutex]::new($false, $mutexName) } catch { }

$ownsMutex = $false
if ($mutex) {
    try { $ownsMutex = $mutex.WaitOne(0) }
    catch [System.Threading.AbandonedMutexException] { $ownsMutex = $true }
}
if (-not $ownsMutex) {
    Write-Log 'another watcher is already running; exit'
    exit 0
}

Write-Log ('watcher start: duration={0}s poll={1}ms alertOnly={2}' -f $DurationSeconds, $PollMilliseconds, [bool]$AlertOnly)
$deadline = (Get-Date).AddSeconds($DurationSeconds)
$dialogSeen = $false
$resolved = $false
$clickLogged = $false

try {
    while ((Get-Date) -lt $deadline) {
        $pros = Get-ProProcesses
        if (-not $pros) {
            Write-Log 'ArcGIS Pro process not found; exit'
            break
        }
        $ids = @($pros | ForEach-Object { $_.Id })
        $dlg = $null
        try { $dlg = Find-TrustDialog $ids } catch { Write-Log ('scan failed: ' + $_.Exception.Message) }

        if ($dlg) {
            $resolved = $false
            if (-not $dialogSeen) {
                $dialogSeen = $true
                $textShort = ($dlg.Text -replace '\s+', ' ')
                if ($textShort.Length -gt 400) { $textShort = $textShort.Substring(0, 400) }
                Write-Log ('dialog detected: win="{0}" text="{1}" buttons=[{2}]' -f $dlg.WinName, $textShort, ($dlg.Buttons -join ' | '))
                # ENVI 导入等流程同样适用：弹窗出现时把 Pro 提到最前，保证确认框是最上层可见窗口
                $raised = Bring-ProToFront $ids $dlg.Window
                Write-Log ('bring Pro to front -> ' + $raised)
            }

            if ($dlg.Button -and -not $AlertOnly) {
                $ok = Invoke-Button $dlg.Button
                if (-not $clickLogged) {
                    Write-Log ('invoke "{0}" -> {1}' -f $dlg.ButtonName, $ok)
                    $clickLogged = $true
                }
                if ($ok) {
                    Start-Sleep -Milliseconds 400
                    $still = $null
                    try { $still = Find-TrustDialog $ids } catch { }
                    if (-not $still) {
                        $resolved = $true
                        Write-Log 'dialog dismissed after auto-click'
                        Close-Alert
                    } else {
                        Show-Alert '自动点击未生效，需要你手动点击。'
                    }
                } else {
                    Show-Alert '自动点击失败，需要你手动点击。'
                }
            } else {
                if ($AlertOnly) { Show-Alert '（AlertOnly 模式：需要你手动点击）' }
                else { Show-Alert '未找到可点击的按钮，需要你手动点击。' }
            }
        } else {
            if ($dialogSeen -and -not $resolved) {
                $resolved = $true
                Write-Log 'dialog no longer detected'
            }
            Close-Alert
        }

        if ($script:AlertForm) {
            [System.Windows.Forms.Application]::DoEvents()
            # 每次轮询把提醒窗重新钉到最顶层（防止被其他置顶窗或全屏窗口压住）
            try {
                $alertHwnd = $script:AlertForm.Handle
                if ($alertHwnd -ne [IntPtr]::Zero) {
                    [void][ArcWpc.Native]::SetWindowPos($alertHwnd, [IntPtr]::new(-1), 0, 0, 0, 0, 0x0013)
                    $script:AlertForm.BringToFront()
                }
            } catch { }
            $now = Get-Date
            if (($now - $script:LastBeep).TotalSeconds -ge 10 -and -not $resolved) {
                [System.Media.SystemSounds]::Exclamation.Play()
                $script:LastBeep = $now
                # 响铃提醒的同时把 Pro 再提到最前，确保用户能看到要点的确认框
                [void](Bring-ProToFront $ids $null)
            }
        }
        Start-Sleep -Milliseconds $PollMilliseconds
    }
} finally {
    # 用户要求：提醒窗至少显示 AlertMinSeconds（≥30）秒，退出时若不足则补足再关
    if ($script:AlertForm) {
        try {
            $remain = $AlertMinSeconds - ((Get-Date) - $script:AlertShownAt).TotalSeconds
            if ($remain -gt 0) {
                $until = (Get-Date).AddSeconds($remain)
                while ((Get-Date) -lt $until) {
                    [System.Windows.Forms.Application]::DoEvents()
                    Start-Sleep -Milliseconds 100
                }
            }
        } catch { }
    }
    Close-Alert -Force
    if ($ownsMutex -and $mutex) { try { $mutex.ReleaseMutex() } catch { } }
    if ($mutex) { $mutex.Dispose() }
    Write-Log 'watcher end'
}
