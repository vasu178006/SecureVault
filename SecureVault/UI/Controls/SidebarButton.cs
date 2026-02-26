// ============================================
// SecureVault - Sidebar Button
// Navigation button for the sidebar
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A sidebar navigation button with icon text and hover/active states.
    /// </summary>
    public class SidebarButton : Button
    {
        private bool _isHovered;
        private bool _isActive;
        private static readonly Font IconFont = new("Segoe UI Emoji", 14);

        public string IconText { get; set; } = "📄"; // Emoji or text icon
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
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

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(AppTheme.GetEffectiveBackColor(Parent));

            var rect = ClientRectangle;

            // Background
            if (_isActive)
            {
                // Active: gradient accent bar + semi-transparent purple background
                using var bgBrush = new SolidBrush(AppTheme.SidebarActive);
                using var bgPath = AppTheme.CreateRoundedRect(new Rectangle(4, 2, rect.Width - 8, rect.Height - 4), 8);
                g.FillPath(bgBrush, bgPath);

                // Left accent bar with gradient
                using var accentBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 4, rect.Height), AppTheme.GradientStart, AppTheme.GradientEnd, 90f);
                using var accentPath = AppTheme.CreateRoundedRect(new Rectangle(0, 6, 3, rect.Height - 12), 2);
                g.FillPath(accentBrush, accentPath);
            }
            else if (_isHovered)
            {
                using var hoverBrush = new SolidBrush(AppTheme.SidebarHover);
                using var hoverPath = AppTheme.CreateRoundedRect(new Rectangle(4, 2, rect.Width - 8, rect.Height - 4), 8);
                g.FillPath(hoverBrush, hoverPath);
            }

            // Icon — use shared static font to avoid allocation per paint
            Color textColor = _isActive ? AppTheme.TextPrimary : (_isHovered ? AppTheme.TextPrimary : AppTheme.TextSecondary);
            TextRenderer.DrawText(g, IconText, IconFont, new Point(16, (rect.Height - 24) / 2), textColor);

            // Text — active items use bold
            var textFont = _isActive ? new Font(Font, FontStyle.Bold) : Font;
            var textRect = new Rectangle(50, 0, rect.Width - 60, rect.Height);
            TextRenderer.DrawText(g, Text, textFont, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            if (_isActive) textFont.Dispose();
        }
    }
}
