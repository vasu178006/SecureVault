// ============================================
// SecureVault - Gradient Panel (Redesigned)
// With inner glow highlight and noise texture
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// Custom Panel with gradient background, inner glow highlight,
    /// and optional rounded corners.
    /// </summary>
    public class GradientPanel : Panel
    {
        public Color GradientStartColor { get; set; } = AppTheme.GradientStart;
        public Color GradientEndColor { get; set; } = AppTheme.GradientEnd;
        public float GradientAngle { get; set; } = 135f;
        public int CornerRadius { get; set; } = 0;
        public bool ShowInnerGlow { get; set; } = true;

        public GradientPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var brush = new LinearGradientBrush(rect, GradientStartColor, GradientEndColor, GradientAngle);

            if (CornerRadius > 0)
            {
                g.Clear(AppTheme.GetEffectiveBackColor(Parent));
                using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);
                g.FillPath(brush, path);

                // Inner glow at top
                if (ShowInnerGlow)
                {
                    var glowRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height / 3);
                    using var glowBrush = new LinearGradientBrush(glowRect,
                        Color.FromArgb(20, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255), 90f);
                    var prevClip = g.Clip;
                    g.SetClip(path, CombineMode.Intersect);
                    g.FillRectangle(glowBrush, glowRect);
                    g.Clip = prevClip;
                }
            }
            else
            {
                g.FillRectangle(brush, rect);

                if (ShowInnerGlow)
                {
                    var glowRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 3);
                    using var glowBrush = new LinearGradientBrush(glowRect,
                        Color.FromArgb(15, 255, 255, 255),
                        Color.FromArgb(0, 255, 255, 255), 90f);
                    g.FillRectangle(glowBrush, glowRect);
                }
            }
        }
    }
}
