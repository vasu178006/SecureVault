// ============================================
// SecureVault - Rounded Button (Redesigned)
// Animated hover/press with glow, smooth color
// transitions, and icon support
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium rounded button with animated gradient fill,
    /// smooth hover transitions, press scale, and glow effect.
    /// </summary>
    public class RoundedButton : Button
    {
        private float _hoverProgress;     // 0 = idle, 1 = fully hovered
        private float _pressProgress;     // 0 = idle, 1 = fully pressed
        private bool _isHovered;
        private bool _isPressed;
        private System.Windows.Forms.Timer? _hoverTimer;
        private System.Windows.Forms.Timer? _pressTimer;

        public Color ButtonColorStart { get; set; } = AppTheme.GradientStart;
        public Color ButtonColorEnd { get; set; } = AppTheme.GradientEnd;
        public Color HoverColorStart { get; set; } = Color.FromArgb(139, 92, 246);   // Lighter violet
        public Color HoverColorEnd { get; set; } = Color.FromArgb(96, 165, 250);     // Lighter blue
        public int CornerRadius { get; set; } = AppTheme.RadiusMedium;
        public bool IsGradient { get; set; } = true;
        public Color FlatColor { get; set; } = AppTheme.GradientStart;
        public string? IconText { get; set; }  // Emoji or text icon drawn left of text

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            BackColor = Color.Transparent;
            ForeColor = AppTheme.TextOnAccent;
            Font = AppTheme.ButtonFont;
            Cursor = Cursors.Hand;
            Size = new Size(160, 46);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            StartHoverAnimation(targetProgress: 1f);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            StartHoverAnimation(targetProgress: 0f);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            StartPressAnimation(targetProgress: 1f);
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            StartPressAnimation(targetProgress: 0f);
            base.OnMouseUp(mevent);
        }

        private void StartHoverAnimation(float targetProgress)
        {
            _hoverTimer?.Stop();
            _hoverTimer?.Dispose();
            _hoverTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float step = targetProgress > _hoverProgress ? 0.12f : -0.12f;
            _hoverTimer.Tick += (s, e) =>
            {
                _hoverProgress += step;
                if ((step > 0 && _hoverProgress >= targetProgress) ||
                    (step < 0 && _hoverProgress <= targetProgress))
                {
                    _hoverProgress = targetProgress;
                    _hoverTimer.Stop();
                    _hoverTimer.Dispose();
                    _hoverTimer = null;
                }
                Invalidate();
            };
            _hoverTimer.Start();
        }

        private void StartPressAnimation(float targetProgress)
        {
            _pressTimer?.Stop();
            _pressTimer?.Dispose();
            _pressTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float step = targetProgress > _pressProgress ? 0.25f : -0.10f;
            _pressTimer.Tick += (s, e) =>
            {
                _pressProgress += step;
                if ((step > 0 && _pressProgress >= targetProgress) ||
                    (step < 0 && _pressProgress <= targetProgress))
                {
                    _pressProgress = targetProgress;
                    _pressTimer.Stop();
                    _pressTimer.Dispose();
                    _pressTimer = null;
                }
                Invalidate();
            };
            _pressTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(AppTheme.GetEffectiveBackColor(Parent));

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Press scale (smooth interpolation)
            int shrink = (int)(3 * _pressProgress);
            if (shrink > 0) rect.Inflate(-shrink, -shrink);

            using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);

            // Glow border on hover
            if (_hoverProgress > 0.01f)
            {
                Color glowColor = IsGradient ? AppTheme.AccentGlow : FlatColor;
                AppTheme.PaintGlowBorder(g, rect, CornerRadius, glowColor, _hoverProgress * 0.6f);
            }

            // Fill
            if (!Enabled)
            {
                // Disabled state
                using var disabledBrush = new SolidBrush(AppTheme.SurfaceMid);
                g.FillPath(disabledBrush, path);
            }
            else if (IsGradient)
            {
                Color start = AppTheme.LerpColor(ButtonColorStart, HoverColorStart, _hoverProgress);
                Color end = AppTheme.LerpColor(ButtonColorEnd, HoverColorEnd, _hoverProgress);
                using var brush = new LinearGradientBrush(rect, start, end, 135f);
                g.FillPath(brush, path);
            }
            else
            {
                Color fill = AppTheme.LerpColor(FlatColor,
                    AppTheme.LerpColor(FlatColor, Color.FromArgb(FlatColor.R + 30, FlatColor.G + 30, FlatColor.B + 30), 1f),
                    _hoverProgress);
                using var brush = new SolidBrush(fill);
                g.FillPath(brush, path);
            }

            // Subtle top highlight for depth
            if (Enabled && _pressProgress < 0.5f)
            {
                var highlightRect = new Rectangle(rect.X + 2, rect.Y + 1, rect.Width - 4, rect.Height / 2);
                using var highlightBrush = new LinearGradientBrush(
                    highlightRect,
                    Color.FromArgb(25, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255), 90f);
                var prevClip = g.Clip;
                g.SetClip(path, CombineMode.Intersect);
                g.FillRectangle(highlightBrush, highlightRect);
                g.Clip = prevClip;
            }

            // Text (with optional icon)
            var textColor = Enabled ? ForeColor : AppTheme.TextMuted;
            if (!string.IsNullOrEmpty(IconText))
            {
                // Measure icon + text
                var iconSize = TextRenderer.MeasureText(g, IconText, Font);
                var textSize = TextRenderer.MeasureText(g, Text, Font);
                int totalWidth = iconSize.Width + textSize.Width - 4;
                int startX = rect.X + (rect.Width - totalWidth) / 2;
                int y = rect.Y + (rect.Height - textSize.Height) / 2;
                TextRenderer.DrawText(g, IconText, Font, new Point(startX, y), textColor);
                TextRenderer.DrawText(g, Text, Font, new Point(startX + iconSize.Width - 4, y), textColor);
            }
            else
            {
                TextRenderer.DrawText(g, Text, Font, rect, textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer?.Stop();
                _hoverTimer?.Dispose();
                _pressTimer?.Stop();
                _pressTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
