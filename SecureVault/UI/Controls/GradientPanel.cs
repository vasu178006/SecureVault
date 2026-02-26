// ============================================
// SecureVault - Gradient Panel
// Custom panel with gradient background
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// Custom Panel with a gradient background.
    /// </summary>
    public class GradientPanel : Panel
    {
        public Color GradientStartColor { get; set; } = AppTheme.GradientStart;
        public Color GradientEndColor { get; set; } = AppTheme.GradientEnd;
        public float GradientAngle { get; set; } = 135f;
        public int CornerRadius { get; set; } = 0;

        public GradientPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
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
                using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);
                g.FillPath(brush, path);
            }
            else
            {
                g.FillRectangle(brush, rect);
            }
        }
    }
}
