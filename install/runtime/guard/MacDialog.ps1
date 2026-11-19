#requires -Version 5.1
<#
macOS 风格弹窗库（2026-11-10）
- 统一样式：无边框圆角窗口、左上红黄绿三个圆点、TopMost、居中/右上角、底部 macOS 风格按钮。
- 仅使用 Windows PowerShell 5.1 自带 WinForms，无第三方依赖。
- 被其他脚本 dot-source 使用；本文件本身不执行任何逻辑。
- 配套 CLI：MacDialogCli.ps1（供 Python / 外部进程调用）。

公开函数:
  Show-MacDialog        - 模态对话框，返回 Yes / No / OK / Timeout / Closed / Unavailable
  New-MacAlertWindow    - 非模态提醒窗（供守护脚本使用），返回 Form（调用方负责 Show/Close）
  Test-MacDialogInteractive - 当前进程是否在交互桌面
#>

$script:MacDialogRoot = $PSScriptRoot
$script:MacDialogLogPath = Join-Path $script:MacDialogRoot 'macdialog.log'

function Write-MacDialogLog {
    param([string]$Message)
    try {
        $line = '[' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + '] ' + $Message
        [System.IO.File]::AppendAllText($script:MacDialogLogPath, $line + [Environment]::NewLine, (New-Object System.Text.UTF8Encoding($false)))
    } catch { }
}

$script:MacDialogCSharp = @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MacDialogUi
{
    public static class Native
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }

    public class ShadowForm : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | 0x20000; // CS_DROPSHADOW
                return cp;
            }
        }
    }

    public class TrafficDot : Control
    {
        private bool hover;
        private Color dotColor;
        private string glyph;

        public Color DotColor
        {
            get { return dotColor; }
            set { dotColor = value; Invalidate(); }
        }

        public string Glyph
        {
            get { return glyph; }
            set { glyph = value; Invalidate(); }
        }

        public TrafficDot()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            Size = new Size(13, 13);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            dotColor = Color.Red;
            glyph = "";
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (SolidBrush b = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(b, r);
            }
            using (Pen p = new Pen(Color.FromArgb(35, 0, 0, 0)))
            {
                e.Graphics.DrawEllipse(p, r);
            }
            if (hover && glyph.Length > 0)
            {
                using (Font f = new Font("Segoe UI", 7.0f, FontStyle.Bold, GraphicsUnit.Point))
                using (SolidBrush tb = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    e.Graphics.DrawString(glyph, f, tb, new RectangleF(0, -0.5f, Width, Height), sf);
                    sf.Dispose();
                }
            }
        }
    }

    public class MacButton : Button
    {
        private bool hover;
        private bool pressed;
        private int cornerRadius;
        private Color fillColor;
        private Color hoverColor;
        private Color pressedColor;
        private Color textColor;
        private Color borderColor;

        public int CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; Invalidate(); }
        }

        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get { return hoverColor; }
            set { hoverColor = value; }
        }

        public Color PressedColor
        {
            get { return pressedColor; }
            set { pressedColor = value; }
        }

        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        public MacButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            // 必须用不透明底色：透明底 + 双缓冲会让圆角外的像素露出黑色底衬（按钮黑边问题）
            BackColor = Color.FromArgb(246, 246, 248);
            Cursor = Cursors.Hand;
            Height = 32;
            Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            fillColor = Color.FromArgb(0, 122, 255);
            hoverColor = Color.FromArgb(10, 132, 255);
            pressedColor = Color.FromArgb(0, 99, 204);
            textColor = Color.White;
            borderColor = Color.Transparent;
            cornerRadius = 7;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { pressed = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        private GraphicsPath RoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // 先用底色铺满整个按钮矩形，避免圆角外区域出现黑边
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill = fillColor;
            if (pressed) { fill = pressedColor; }
            else if (hover) { fill = hoverColor; }
            using (GraphicsPath path = RoundRect(rect, cornerRadius))
            {
                using (SolidBrush b = new SolidBrush(fill))
                {
                    e.Graphics.FillPath(b, path);
                }
                if (borderColor != Color.Transparent)
                {
                    using (Pen p = new Pen(borderColor, 1.0f))
                    {
                        e.Graphics.DrawPath(p, path);
                    }
                }
            }
            if (Focused)
            {
                Rectangle fr = new Rectangle(2, 2, Width - 5, Height - 5);
                // 蓝色主按钮上的蓝环不可见：按填充色亮度选择对比色
                int lum = (fill.R * 299 + fill.G * 587 + fill.B * 114) / 1000;
                Color ring = lum < 128
                    ? Color.FromArgb(220, 255, 255, 255)
                    : Color.FromArgb(120, 0, 122, 255);
                using (GraphicsPath fp = RoundRect(fr, cornerRadius - 1))
                using (Pen p = new Pen(ring, 1.5f))
                {
                    e.Graphics.DrawPath(p, fp);
                }
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, rect, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }
    }
}
'@

function Test-MacDialogInteractive {
    try {
        if (-not [Environment]::UserInteractive) { return $false }
        $p = Get-Process -Id $PID -ErrorAction SilentlyContinue
        if ($p -and $null -ne $p.SessionId -and [int]$p.SessionId -eq 0) { return $false }
        return $true
    } catch {
        return $false
    }
}

function Initialize-MacDialogUi {
    if ('MacDialogUi.ShadowForm' -as [type]) { return }
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    Add-Type -TypeDefinition $script:MacDialogCSharp -ReferencedAssemblies 'System.Windows.Forms', 'System.Drawing'
}

function New-MacRoundedPath {
    param([int]$Width, [int]$Height, [int]$Radius)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $Radius * 2
    if ($d -le 0) {
        $path.AddRectangle([System.Drawing.Rectangle]::new(0, 0, $Width, $Height))
        return $path
    }
    $path.AddArc(0, 0, $d, $d, 180, 90)
    $path.AddArc($Width - $d, 0, $d, $d, 270, 90)
    $path.AddArc($Width - $d, $Height - $d, $d, $d, 0, 90)
    $path.AddArc(0, $Height - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function Set-MacRoundedRegion {
    param([System.Windows.Forms.Form]$Form, [int]$Radius = 12)
    try {
        $path = New-MacRoundedPath -Width $Form.Width -Height $Form.Height -Radius $Radius
        $Form.Region = New-Object System.Drawing.Region($path)
        $path.Dispose()
    } catch { }
}

function New-MacChrome {
    param(
        [string]$Title = '提示',
        [string]$Message = '',
        [int]$Width = 600,
        [int]$Height = 300,
        [ValidateSet('CenterScreen', 'TopRight')][string]$Position = 'CenterScreen',
        [switch]$TopMost,
        [switch]$ShowInTaskbar
    )

    Initialize-MacDialogUi

    $Message = ($Message -replace "`r?`n", "`r`n")

    $form = New-Object MacDialogUi.ShadowForm
    $form.Text = $Title
    $form.TopMost = [bool]$TopMost
    $form.ShowInTaskbar = [bool]$ShowInTaskbar
    $form.FormBorderStyle = 'None'
    $form.AutoScaleMode = 'Font'
    $form.ClientSize = [System.Drawing.Size]::new($Width, $Height)
    $form.BackColor = [System.Drawing.Color]::FromArgb(246, 246, 248)
    $form.KeyPreview = $true

    $state = @{
        Form      = $form
        Kind      = 'Alert'
        Result    = ''
        Remain    = 0
        Timer     = $null
        Countdown = $null
        DefaultYes = $false
    }

    $titlePanel = New-Object System.Windows.Forms.Panel
    $titlePanel.Dock = 'Top'
    $titlePanel.Height = 52
    $titlePanel.BackColor = $form.BackColor

    $line = New-Object System.Windows.Forms.Panel
    $line.Dock = 'Bottom'
    $line.Height = 1
    $line.BackColor = [System.Drawing.Color]::FromArgb(229, 229, 234)
    $titlePanel.Controls.Add($line)

    $titleLabel = New-Object System.Windows.Forms.Label
    $titleLabel.Dock = 'Fill'
    $titleLabel.TextAlign = 'MiddleCenter'
    $titleLabel.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 12.5, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Point)
    $titleLabel.ForeColor = [System.Drawing.Color]::FromArgb(29, 29, 31)
    $titleLabel.Padding = [System.Windows.Forms.Padding]::new(86, 0, 86, 0)
    $titleLabel.Text = $Title
    $titlePanel.Controls.Add($titleLabel)

    $state.TitleLabel = $titleLabel

    $makeDot = {
        param([System.Drawing.Color]$Color, [string]$Glyph, [int]$X)
        $dot = New-Object MacDialogUi.TrafficDot
        $dot.DotColor = $Color
        $dot.Glyph = $Glyph
        $dot.Location = [System.Drawing.Point]::new($X, 19)
        return $dot
    }

    $red = & $makeDot ([System.Drawing.Color]::FromArgb(255, 95, 87)) ([char]0x00D7) 18
    $yellow = & $makeDot ([System.Drawing.Color]::FromArgb(254, 188, 46)) ([char]0x2212) 38
    $green = & $makeDot ([System.Drawing.Color]::FromArgb(40, 200, 64)) '+' 58

    $red.Add_Click({
        $state.Result = 'Closed'
        try { $state.Form.Close() } catch { }
    }.GetNewClosure())

    $yellow.Add_Click({
        try { $state.Form.WindowState = [System.Windows.Forms.FormWindowState]::Minimized } catch { }
    }.GetNewClosure())

    $green.Add_Click({
        if ($state.Kind -eq 'Question') { $state.Result = 'Yes' } else { $state.Result = 'OK' }
        try { $state.Form.Close() } catch { }
    }.GetNewClosure())

    $titlePanel.Controls.Add($red)
    $titlePanel.Controls.Add($yellow)
    $titlePanel.Controls.Add($green)
    $red.BringToFront()
    $yellow.BringToFront()
    $green.BringToFront()

    $state.Red = $red
    $state.Yellow = $yellow
    $state.Green = $green

    $dragHandler = {
        if ($_.Button -eq [System.Windows.Forms.MouseButtons]::Left) {
            try { [MacDialogUi.Native]::ReleaseCapture() | Out-Null } catch { }
            try { [MacDialogUi.Native]::SendMessage($state.Form.Handle, 0xA1, [IntPtr]0x2, [IntPtr]::Zero) | Out-Null } catch { }
        }
    }.GetNewClosure()
    $titlePanel.Add_MouseDown($dragHandler)
    $titleLabel.Add_MouseDown($dragHandler)

    $bodyPanel = New-Object System.Windows.Forms.Panel
    $bodyPanel.Dock = 'Fill'
    $bodyPanel.AutoScroll = $true
    $bodyPanel.BackColor = $form.BackColor
    $bodyPanel.Padding = [System.Windows.Forms.Padding]::new(26, 18, 26, 12)

    $body = New-Object System.Windows.Forms.Label
    $body.AutoSize = $true
    $body.MaximumSize = [System.Drawing.Size]::new(($Width - 70), 0)
    $body.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 10, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Point)
    $body.ForeColor = [System.Drawing.Color]::FromArgb(29, 29, 31)
    $body.Text = $Message
    $body.Location = [System.Drawing.Point]::new(26, 18)
    $bodyPanel.Controls.Add($body)

    $bottom = New-Object System.Windows.Forms.Panel
    $bottom.Dock = 'Bottom'
    $bottom.Height = 72
    $bottom.BackColor = $form.BackColor

    $countdown = New-Object System.Windows.Forms.Label
    $countdown.AutoSize = $true
    $countdown.Location = [System.Drawing.Point]::new(26, 30)
    $countdown.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 8.5, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Point)
    $countdown.ForeColor = [System.Drawing.Color]::FromArgb(134, 134, 139)
    $bottom.Controls.Add($countdown)

    $state.Countdown = $countdown
    $state.Body = $body
    $state.Bottom = $bottom
    $state.TitlePanel = $titlePanel
    $state.BodyPanel = $bodyPanel

    $form.Controls.Add($bodyPanel)
    $form.Controls.Add($bottom)
    $form.Controls.Add($titlePanel)

    $screen = [System.Windows.Forms.Screen]::FromPoint([System.Windows.Forms.Cursor]::Position)
    $area = $screen.WorkingArea
    if ($Position -eq 'TopRight') {
        $form.StartPosition = 'Manual'
        $form.Location = [System.Drawing.Point]::new([int]($area.Right - $Width - 24), [int]($area.Top + 24))
    } else {
        $form.StartPosition = 'Manual'
        $form.Location = [System.Drawing.Point]::new(
            [int]($area.Left + ($area.Width - $Width) / 2),
            [int]($area.Top + ($area.Height - $Height) / 2))
    }

    $form.Add_Shown({
        try { $state.Form.Activate() } catch { }
        try { [MacDialogUi.Native]::SetForegroundWindow($state.Form.Handle) | Out-Null } catch { }
        try {
            $v = 2
            $hr = [MacDialogUi.Native]::DwmSetWindowAttribute($state.Form.Handle, 33, [ref]$v, 4)
            if ($hr -ne 0) { Set-MacRoundedRegion -Form $state.Form -Radius 12 }
        } catch {
            Set-MacRoundedRegion -Form $state.Form -Radius 12
        }
    }.GetNewClosure())

    $form.Add_FormClosing({
        if (-not $state.Result) { $state.Result = 'Closed' }
        try { if ($state.Timer) { $state.Timer.Stop() } } catch { }
    }.GetNewClosure())

    return $state
}

function Show-MacDialog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Message,
        [ValidateSet('Question', 'Alert')][string]$Kind = 'Alert',
        [string]$YesText = '是',
        [string]$NoText = '否',
        [string]$OkText = '知道了',
        [int]$TimeoutSeconds = 60,
        [switch]$DefaultYes,
        [switch]$DefaultNo,
        [switch]$NoSound
    )

    if (-not (Test-MacDialogInteractive)) { return 'Unavailable' }

    Write-MacDialogLog ('Show-MacDialog start: kind=' + $Kind + ' title=' + $Title + ' timeout=' + $TimeoutSeconds)

    $mutex = $null
    $acquired = $false
    try {
        $mutex = New-Object System.Threading.Mutex($false, 'Global\MacDialogUi')
        try { $acquired = $mutex.WaitOne(3000) }
        catch [System.Threading.AbandonedMutexException] { $acquired = $true }
    } catch {
        $acquired = $false
    }
    if (-not $acquired) {
        Write-MacDialogLog 'Show-MacDialog: mutex unavailable'
        if ($mutex) { try { $mutex.Dispose() } catch { } }
        return 'Unavailable'
    }

    try {
        $lineCount = @($Message -split "`r?`n").Count
        $height = 250 + [Math]::Min(260, [Math]::Max(0, $lineCount - 3) * 20)
        if ($height -gt 560) { $height = 560 }

        $state = New-MacChrome -Title $Title -Message $Message -Width 600 -Height $height -Position 'CenterScreen' -TopMost -ShowInTaskbar
        $state.Kind = $Kind
        $state.DefaultYes = [bool]$DefaultYes

        $btnWidth = 94
        $right = 600 - 26
        if ($Kind -eq 'Question') {
            $btnNo = New-Object MacDialogUi.MacButton
            $btnNo.Text = $NoText
            $btnNo.Width = $btnWidth
            $btnNo.Height = 32
            $btnNo.FillColor = [System.Drawing.Color]::FromArgb(232, 232, 237)
            $btnNo.HoverColor = [System.Drawing.Color]::FromArgb(220, 220, 225)
            $btnNo.PressedColor = [System.Drawing.Color]::FromArgb(210, 210, 216)
            $btnNo.TextColor = [System.Drawing.Color]::FromArgb(29, 29, 31)
            $btnNo.Location = [System.Drawing.Point]::new(($right - $btnWidth - 12 - $btnWidth), 20)
            $btnNo.Add_Click({
                $state.Result = 'No'
                try { $state.Form.Close() } catch { }
            }.GetNewClosure())
            $state.Bottom.Controls.Add($btnNo)

            $btnYes = New-Object MacDialogUi.MacButton
            $btnYes.Text = $YesText
            $btnYes.Width = $btnWidth
            $btnYes.Height = 32
            $btnYes.Location = [System.Drawing.Point]::new(($right - $btnWidth), 20)
            $btnYes.Add_Click({
                $state.Result = 'Yes'
                try { $state.Form.Close() } catch { }
            }.GetNewClosure())
            $state.Bottom.Controls.Add($btnYes)

            $state.Form.AcceptButton = $btnYes
            $state.Form.CancelButton = $btnNo
            if ($DefaultYes) {
                $state.Form.AcceptButton = $btnYes
            } else {
                $state.Form.AcceptButton = $btnNo
            }
            # 显示前 Focus() 不生效（CanFocus=False），改在 Shown 后再聚焦
            $state.Form.Add_Shown({
                try {
                    if ($state.DefaultYes) { [void]$btnYes.Focus() } else { [void]$btnNo.Focus() }
                } catch { }
            }.GetNewClosure())
            $state.PrimaryButton = $btnYes
            $state.SecondaryButton = $btnNo
        } else {
            $btnOk = New-Object MacDialogUi.MacButton
            $btnOk.Text = $OkText
            $btnOk.Width = 108
            $btnOk.Height = 32
            $btnOk.Location = [System.Drawing.Point]::new(($right - 108), 20)
            $btnOk.Add_Click({
                $state.Result = 'OK'
                try { $state.Form.Close() } catch { }
            }.GetNewClosure())
            $state.Bottom.Controls.Add($btnOk)
            $state.Form.AcceptButton = $btnOk
            $state.Form.CancelButton = $btnOk
            $state.Form.Add_Shown({ try { [void]$btnOk.Focus() } catch { } }.GetNewClosure())
            $state.PrimaryButton = $btnOk
        }

        $defaultLabel = $NoText
        if ($DefaultYes) { $defaultLabel = $YesText }
        if ($TimeoutSeconds -gt 0) {
            $state.Remain = [int]$TimeoutSeconds
            if ($Kind -eq 'Question') {
                $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动选择「' + $defaultLabel + '」')
            } else {
                $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动关闭')
            }
            $timer = New-Object System.Windows.Forms.Timer
            $timer.Interval = 1000
            $timer.Add_Tick({
                $state.Remain--
                if ($state.Remain -le 0) {
                    if ($state.Kind -eq 'Question' -and $state.DefaultYes) {
                        $state.Result = 'Yes'
                    } else {
                        $state.Result = 'Timeout'
                    }
                    try { $state.Timer.Stop() } catch { }
                    try { $state.Form.Close() } catch { }
                    return
                }
                if ($state.Kind -eq 'Question') {
                    $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动选择「' + $defaultLabel + '」')
                } else {
                    $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动关闭')
                }
            }.GetNewClosure())
            $state.Timer = $timer
            $state.Form.Add_Shown({
                try { if ($state.Timer) { $state.Timer.Start() } } catch { }
            }.GetNewClosure())
        } else {
            $state.Countdown.Text = '等待你的操作'
        }

        if (-not $NoSound) {
            try {
                if ($Kind -eq 'Question') { [System.Media.SystemSounds]::Question.Play() }
                else { [System.Media.SystemSounds]::Asterisk.Play() }
            } catch { }
        }

        [void]$state.Form.ShowDialog()
        $result = $state.Result
        if (-not $result) { $result = 'Closed' }
        try { $state.Form.Dispose() } catch { }
        Write-MacDialogLog ('Show-MacDialog end: ' + $result)
        return $result
    } catch {
        Write-MacDialogLog ('Show-MacDialog error: ' + $_.Exception.Message + ' | ' + $_.ScriptStackTrace)
        return 'Unavailable'
    } finally {
        if ($mutex -and $acquired) { try { $mutex.ReleaseMutex() } catch { } }
        if ($mutex) { try { $mutex.Dispose() } catch { } }
    }
}

function New-MacAlertWindow {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Message,
        [int]$Width = 560,
        [int]$Height = 210,
        [ValidateSet('CenterScreen', 'TopRight')][string]$Position = 'TopRight'
    )

    if (-not (Test-MacDialogInteractive)) { return $null }

    try {
        $state = New-MacChrome -Title $Title -Message $Message -Width $Width -Height $Height -Position $Position -TopMost -ShowInTaskbar
        $state.Kind = 'Alert'
        $state.Countdown.Text = '等待你的操作'

        $btnClose = New-Object MacDialogUi.MacButton
        $btnClose.Text = '关闭'
        $btnClose.Width = 84
        $btnClose.Height = 32
        $btnClose.FillColor = [System.Drawing.Color]::FromArgb(232, 232, 237)
        $btnClose.HoverColor = [System.Drawing.Color]::FromArgb(220, 220, 225)
        $btnClose.PressedColor = [System.Drawing.Color]::FromArgb(210, 210, 216)
        $btnClose.TextColor = [System.Drawing.Color]::FromArgb(29, 29, 31)
        $btnClose.Location = [System.Drawing.Point]::new(($Width - 26 - 84), 20)
        $btnClose.Add_Click({
            $state.Result = 'Closed'
            try { $state.Form.Close() } catch { }
        }.GetNewClosure())
        $state.Bottom.Controls.Add($btnClose)
        $state.Form.CancelButton = $btnClose

        return $state.Form
    } catch {
        Write-MacDialogLog ('New-MacAlertWindow error: ' + $_.Exception.Message + ' | ' + $_.ScriptStackTrace)
        return $null
    }
}

function Show-MacInput {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Message,
        [string]$DefaultValue = '',
        [string]$OkText = '确定',
        [string]$CancelText = '取消',
        [int]$TimeoutSeconds = 0
    )

    if (-not (Test-MacDialogInteractive)) { return $null }

    $mutex = $null
    $acquired = $false
    try {
        $mutex = New-Object System.Threading.Mutex($false, 'Global\MacDialogUi')
        try { $acquired = $mutex.WaitOne(3000) }
        catch [System.Threading.AbandonedMutexException] { $acquired = $true }
    } catch { $acquired = $false }
    if (-not $acquired) {
        if ($mutex) { try { $mutex.Dispose() } catch { } }
        return $null
    }

    try {
        $state = New-MacChrome -Title $Title -Message $Message -Width 600 -Height 340 -Position 'CenterScreen' -TopMost -ShowInTaskbar
        $state.Kind = 'Input'
        $state.Countdown.Text = '等待你的操作'

        $textBox = New-Object System.Windows.Forms.TextBox
        $textBox.Multiline = $true
        $textBox.ScrollBars = [System.Windows.Forms.ScrollBars]::Vertical
        $textBox.Font = [System.Drawing.Font]::new('Microsoft YaHei UI', 10, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Point)
        $textBox.Location = [System.Drawing.Point]::new(26, [Math]::Max(48, [int]$state.Body.Bottom + 12))
        $textBox.Size = [System.Drawing.Size]::new(548, 86)
        $textBox.Text = $DefaultValue
        $state.BodyPanel.Controls.Add($textBox)
        $state.TextBox = $textBox

        $right = 600 - 26
        $btnCancel = New-Object MacDialogUi.MacButton
        $btnCancel.Text = $CancelText
        $btnCancel.Width = 94
        $btnCancel.Height = 32
        $btnCancel.FillColor = [System.Drawing.Color]::FromArgb(232, 232, 237)
        $btnCancel.HoverColor = [System.Drawing.Color]::FromArgb(220, 220, 225)
        $btnCancel.PressedColor = [System.Drawing.Color]::FromArgb(210, 210, 216)
        $btnCancel.TextColor = [System.Drawing.Color]::FromArgb(29, 29, 31)
        $btnCancel.Location = [System.Drawing.Point]::new(($right - 94 - 12 - 94), 20)
        $btnCancel.Add_Click({
            $state.Result = 'Cancel'
            try { $state.Form.Close() } catch { }
        }.GetNewClosure())
        $state.Bottom.Controls.Add($btnCancel)

        $btnOk = New-Object MacDialogUi.MacButton
        $btnOk.Text = $OkText
        $btnOk.Width = 94
        $btnOk.Height = 32
        $btnOk.Location = [System.Drawing.Point]::new(($right - 94), 20)
        $btnOk.Add_Click({
            $state.Result = 'OK'
            try { $state.Form.Close() } catch { }
        }.GetNewClosure())
        $state.Bottom.Controls.Add($btnOk)

        $state.Form.AcceptButton = $btnOk
        $state.Form.CancelButton = $btnCancel

        if ($TimeoutSeconds -gt 0) {
            $state.Remain = [int]$TimeoutSeconds
            $timer = New-Object System.Windows.Forms.Timer
            $timer.Interval = 1000
            $timer.Add_Tick({
                $state.Remain--
                if ($state.Remain -le 0) {
                    $state.Result = 'Cancel'
                    try { $state.Timer.Stop() } catch { }
                    try { $state.Form.Close() } catch { }
                    return
                }
                $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动取消')
            }.GetNewClosure())
            $state.Timer = $timer
            $state.Form.Add_Shown({ try { if ($state.Timer) { $state.Timer.Start() } } catch { } }.GetNewClosure())
            $state.Countdown.Text = ('' + $state.Remain + ' 秒后自动取消')
        }

        $state.Form.Add_Shown({
            try { [void]$state.TextBox.Focus() } catch { }
        }.GetNewClosure())

        [void]$state.Form.ShowDialog()
        $result = $state.Result
        $text = [string]$state.TextBox.Text
        try { $state.Form.Dispose() } catch { }
        if ($result -eq 'OK') { return $text }
        return $null
    } catch {
        Write-MacDialogLog ('Show-MacInput error: ' + $_.Exception.Message + ' | ' + $_.ScriptStackTrace)
        return $null
    } finally {
        if ($mutex -and $acquired) { try { $mutex.ReleaseMutex() } catch { } }
        if ($mutex) { try { $mutex.Dispose() } catch { } }
    }
}
