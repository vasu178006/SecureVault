// ============================================
// SecureVault - Sidebar Button (Redesigned)
// Animated hover/active transitions with
// smooth indicator bar and glow
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium sidebar navigation button with smooth color
    /// transitions, animated active indicator, and icon support.
    /// </summary>
    public class SidebarButton : Button
    {
        private float _hoverProgress;
        private float _activeProgress;
        private bool _isHovered;
        private bool _isActive;
        private System.Windows.Forms.Timer? _hoverTimer;
        private System.Windows.Forms.Timer? _activeTimer;
        private static readonly Font IconFont = new("Segoe UI Emoji", 14);

        public string IconText { get; set; } = "📄";
        public bool IsActive
        {
            get => _isActive;
            set
            {
                bool changed = _isActive != value;
                _isActive = value;
                if (changed) StartActiveAnimation(value ? 1f : 0f);
                Invalidate();
            }
        }

        public SidebarButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            BackColor = Color.Transparent;
            ForeColor = AppTheme.TextSecondary;
            Font = AppTheme.SidebarFont;
            Cursor = Cursors.Hand;
            Size = new Size(240, 46);
            TextAlign = ContentAlignment.MiddleLeft;
            Padding = new Padding(50, 0, 10, 0);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            StartHoverAnimation(1f);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            StartHoverAnimation(0f);
            base.OnMouseLeave(e);
        }

        private void StartHoverAnimation(float target)
        {
            _hoverTimer?.Stop();
            _hoverTimer?.Dispose();
            _hoverTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float step = target > _hoverProgress ? 0.14f : -0.14f;
            _hoverTimer.Tick += (s, e) =>
            {
                _hoverProgress += step;
                if ((step > 0 && _hoverProgress >= target) ||
                    (step < 0 && _hoverProgress <= target))
                {
                    _hoverProgress = target;
                    _hoverTimer.Stop();
                    _hoverTimer.Dispose();
                    _hoverTimer = null;
                }
                Invalidate();
            };
            _hoverTimer.Start();
        }

        private void StartActiveAnimation(float target)
        {
            _activeTimer?.Stop();
            _activeTimer?.Dispose();
            _activeTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float step = target > _activeProgress ? 0.12f : -0.18f;
            _activeTimer.Tick += (s, e) =>
            {
                _activeProgress += step;
                if ((step > 0 && _activeProgress >= target) ||
                    (step < 0 && _activeProgress <= target))
                {
                    _activeProgress = target;
                    _activeTimer.Stop();
                    _activeTimer.Dispose();
                    _activeTimer = null;
                }
                Invalidate();
            };
            _activeTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(AppTheme.GetEffectiveBackColor(Parent));

            var rect = ClientRectangle;

            // Background (hover + active combined)
            float bgProgress = Math.Max(_hoverProgress * 0.6f, _activeProgress);
            if (bgProgress > 0.01f)
            {
                int alpha = (int)(bgProgress * 40);
                using var bgBrush = new SolidBrush(Color.FromArgb(alpha,
                    AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B));
                using var bgPath = AppTheme.CreateRoundedRect(
                    new Rectangle(4, 2, rect.Width - 8, rect.Height - 4), 10);
                g.FillPath(bgBrush, bgPath);
            }

            // Animated left accent bar
            if (_activeProgress > 0.01f)
            {
                int barHeight = (int)((rect.Height - 16) * _activeProgress);
                int barY = (rect.Height - barHeight) / 2;
                var barRect = new Rectangle(0, barY, 3, barHeight);
                if (barHeight > 0)
                {
                    using var accentBrush = new LinearGradientBrush(
                        new Rectangle(0, barY, 4, barHeight),
                        AppTheme.GradientStart, AppTheme.GradientEnd, 90f);
                    using var accentPath = AppTheme.CreateRoundedRect(barRect, 2);
                    g.FillPath(accentBrush, accentPath);
                }
            }

            // Text color
            float colorProgress = Math.Max(_hoverProgress, _activeProgress);
            Color textColor = AppTheme.LerpColor(AppTheme.TextSecondary, AppTheme.TextPrimary, colorProgress);

            // Icon
            TextRenderer.DrawText(g, IconText, IconFont,
                new Point(16, (rect.Height - 24) / 2), textColor);

            // Text — active items use semibold
            var textFont = _isActive ? new Font(Font, FontStyle.Bold) : Font;
            var textRect = new Rectangle(50, 0, rect.Width - 60, rect.Height);
            TextRenderer.DrawText(g, Text, textFont, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            if (_isActive) textFont.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer?.Stop();
                _hoverTimer?.Dispose();
                _activeTimer?.Stop();
                _activeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
