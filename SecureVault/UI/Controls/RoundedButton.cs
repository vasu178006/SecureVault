// ============================================
// SecureVault - Rounded Button
// Modern styled button with gradient and hover effects
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A modern rounded button with gradient fill and hover animation.
    /// </summary>
    public class RoundedButton : Button
    {
        private bool _isHovered;
        private bool _isPressed;

        public Color ButtonColorStart { get; set; } = AppTheme.GradientStart;
        public Color ButtonColorEnd { get; set; } = AppTheme.GradientEnd;
        public Color HoverColorStart { get; set; } = Color.FromArgb(128, 79, 190);
        public Color HoverColorEnd { get; set; } = Color.FromArgb(57, 119, 255);
        public int CornerRadius { get; set; } = AppTheme.RadiusMedium;
        public bool IsGradient { get; set; } = true;
        public Color FlatColor { get; set; } = AppTheme.GradientStart;

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
            Size = new Size(160, 44);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.PrimaryDark);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Scale down slightly when pressed
            if (_isPressed)
            {
                rect.Inflate(-2, -2);
            }

            using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);

            // Fill
            if (IsGradient)
            {
                Color start = _isHovered ? HoverColorStart : ButtonColorStart;
                Color end = _isHovered ? HoverColorEnd : ButtonColorEnd;
                using var brush = new LinearGradientBrush(rect, start, end, 90f);
                g.FillPath(brush, path);
            }
            else
            {
                Color fill = _isHovered ? ControlPaint.Light(FlatColor, 0.2f) : FlatColor;
                using var brush = new SolidBrush(fill);
                g.FillPath(brush, path);
            }

            // Glow border on hover
            if (_isHovered)
            {
                using var glowPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f);
                g.DrawPath(glowPen, path);
            }

            // Text
            var textRect = rect;
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
