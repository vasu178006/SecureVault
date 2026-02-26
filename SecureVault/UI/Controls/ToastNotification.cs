// ============================================
// SecureVault - Toast Notification
// Slide-in from bottom-right, auto-dismiss,
// themed by type (success/error/warning/info)
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    public enum ToastType { Success, Error, Warning, Info }

    /// <summary>
    /// A premium toast notification that slides in from the bottom-right,
    /// auto-dismisses, and provides non-blocking user feedback.
    /// </summary>
    public class ToastNotification : Form
    {
        private readonly System.Windows.Forms.Timer _dismissTimer;
        private readonly System.Windows.Forms.Timer _animTimer;
        private readonly string _message;
        private readonly ToastType _type;
        private int _targetY;
        private bool _isClosing;

        private static readonly List<ToastNotification> _activeToasts = new();

        public static void Show(string message, ToastType type = ToastType.Info, int durationMs = 3500)
        {
            var toast = new ToastNotification(message, type, durationMs);
            toast.ShowToast();
        }

        private ToastNotification(string message, ToastType type, int durationMs)
        {
            _message = message;
            _type = type;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            BackColor = AppTheme.SurfaceDark;
            Size = new Size(360, 60);
            Opacity = 0.95;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // Auto-dismiss timer
            _dismissTimer = new System.Windows.Forms.Timer { Interval = durationMs };
            _dismissTimer.Tick += (s, e) => { _dismissTimer.Stop(); SlideOut(); };

            // Animation timer
            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };

            Click += (s, e) => SlideOut();
        }

        private void ShowToast()
        {
            // Position at bottom-right of primary screen
            var screen = Screen.PrimaryScreen!.WorkingArea;

            // Stack below existing toasts
            int stackOffset = 0;
            lock (_activeToasts)
            {
                foreach (var existing in _activeToasts)
                    stackOffset += existing.Height + 8;
                _activeToasts.Add(this);
            }

            Left = screen.Right - Width - 20;
            _targetY = screen.Bottom - Height - 20 - stackOffset;
            Top = screen.Bottom + 10; // Start off-screen

            Show();
            SlideIn();
            _dismissTimer.Start();
        }

        private void SlideIn()
        {
            int startY = Top;
            int step = 0;
            int totalSteps = 20; // ~320ms
            _animTimer.Tick += SlideInTick;
            _animTimer.Start();

            void SlideInTick(object? s, EventArgs e)
            {
                step++;
                double progress = AnimationHelper.EaseOutQuint((double)step / totalSteps);
                Top = startY + (int)((_targetY - startY) * progress);
                if (step >= totalSteps)
                {
                    Top = _targetY;
                    _animTimer.Tick -= SlideInTick;
                    _animTimer.Stop();
                }
            }
        }

        private void SlideOut()
        {
            if (_isClosing) return;
            _isClosing = true;
            _dismissTimer.Stop();

            int startX = Left;
            int targetX = startX + Width + 30;
            int step = 0;
            int totalSteps = 15; // ~240ms
            _animTimer.Tick += SlideOutTick;
            _animTimer.Start();

            void SlideOutTick(object? s, EventArgs e)
            {
                step++;
                double progress = AnimationHelper.EaseInCubic((double)step / totalSteps);
                Left = startX + (int)((targetX - startX) * progress);
                Opacity = 0.95 * (1 - progress);
                if (step >= totalSteps)
                {
                    _animTimer.Tick -= SlideOutTick;
                    _animTimer.Stop();
                    lock (_activeToasts) { _activeToasts.Remove(this); }
                    Close();
                    Dispose();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = AppTheme.CreateRoundedRect(rect, 12);

            // Background
            using var bgBrush = new SolidBrush(Color.FromArgb(240, AppTheme.SurfaceDark.R, AppTheme.SurfaceDark.G, AppTheme.SurfaceDark.B));
            g.FillPath(bgBrush, path);

            // Border
            using var borderPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f);
            g.DrawPath(borderPen, path);

            // Accent bar on left
            Color accentColor = _type switch
            {
                ToastType.Success => AppTheme.Success,
                ToastType.Error => AppTheme.Error,
                ToastType.Warning => AppTheme.Warning,
                _ => AppTheme.Info
            };
            var accentRect = new Rectangle(0, 0, 4, Height);
            using var accentClip = AppTheme.CreateRoundedRect(rect, 12);
            var prevClip = g.Clip;
            g.SetClip(accentClip, CombineMode.Intersect);
            using var accentBrush = new SolidBrush(accentColor);
            g.FillRectangle(accentBrush, accentRect);
            g.Clip = prevClip;

            // Icon
            string icon = _type switch
            {
                ToastType.Success => "✅",
                ToastType.Error => "❌",
                ToastType.Warning => "⚠️",
                _ => "ℹ️"
            };
            TextRenderer.DrawText(g, icon, new Font("Segoe UI Emoji", 14),
                new Rectangle(14, 0, 30, Height), AppTheme.TextPrimary,
                TextFormatFlags.VerticalCenter);

            // Message
            TextRenderer.DrawText(g, _message, AppTheme.BodyRegular,
                new Rectangle(48, 0, Width - 60, Height), AppTheme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW — no taskbar icon
                return cp;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dismissTimer?.Dispose();
                _animTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
