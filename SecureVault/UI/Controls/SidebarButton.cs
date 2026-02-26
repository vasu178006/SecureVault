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
                     ControlStyles.DoubleBuffer, true);
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
            g.Clear(Parent?.BackColor ?? AppTheme.SidebarBg);

            var rect = ClientRectangle;

            // Background
            if (_isActive)
            {
                // Active: gradient accent bar + subtle background
                using var bgBrush = new SolidBrush(Color.FromArgb(30, AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B));
                using var bgPath = AppTheme.CreateRoundedRect(new Rectangle(4, 2, rect.Width - 8, rect.Height - 4), 8);
                g.FillPath(bgBrush, bgPath);

                // Left accent bar
                using var accentBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 4, rect.Height), AppTheme.GradientStart, AppTheme.GradientEnd, 90f);
                g.FillRectangle(accentBrush, 0, 6, 3, rect.Height - 12);
            }
            else if (_isHovered)
            {
                using var hoverBrush = new SolidBrush(AppTheme.SidebarHover);
                using var hoverPath = AppTheme.CreateRoundedRect(new Rectangle(4, 2, rect.Width - 8, rect.Height - 4), 8);
                g.FillPath(hoverBrush, hoverPath);
            }

            // Icon
            Color textColor = _isActive ? AppTheme.TextPrimary : (_isHovered ? AppTheme.TextPrimary : AppTheme.TextSecondary);
            TextRenderer.DrawText(g, IconText, new Font("Segoe UI Emoji", 14), new Point(16, (rect.Height - 24) / 2), textColor);

            // Text
            var textRect = new Rectangle(50, 0, rect.Width - 60, rect.Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }
}
