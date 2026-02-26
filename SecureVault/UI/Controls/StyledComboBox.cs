// ============================================
// SecureVault - Styled ComboBox
// Owner-drawn dark-themed ComboBox with no ugly borders
// ============================================

using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A custom owner-drawn ComboBox styled with the dark theme.
    /// Eliminates the ugly white/system borders of the default ComboBox.
    /// </summary>
    public class StyledComboBox : ComboBox
    {
        private bool _isHovered;

        public StyledComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            BackColor = AppTheme.SurfaceDark;
            ForeColor = AppTheme.TextPrimary;
            FlatStyle = FlatStyle.Flat;
            Font = AppTheme.BodyRegular;
            ItemHeight = 28;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Background
            using var bgPath = AppTheme.CreateRoundedRect(rect, 6);
            using var bgBrush = new SolidBrush(_isHovered ? AppTheme.SurfaceMid : AppTheme.SurfaceDark);
            g.FillPath(bgBrush, bgPath);

            // Border
            var borderColor = _isHovered
                ? Color.FromArgb(100, AppTheme.AccentGlow.R, AppTheme.AccentGlow.G, AppTheme.AccentGlow.B)
                : AppTheme.SurfaceBorder;
            using var borderPen = new Pen(borderColor, 1f);
            g.DrawPath(borderPen, bgPath);

            // Selected text
            string text = SelectedItem?.ToString() ?? "";
            using var textBrush = new SolidBrush(AppTheme.TextPrimary);
            var textRect = new Rectangle(10, 0, Width - 34, Height);
            var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            g.DrawString(text, Font, textBrush, textRect, sf);

            // Dropdown arrow
            int arrowX = Width - 22;
            int arrowY = Height / 2 - 2;
            using var arrowBrush = new SolidBrush(AppTheme.TextSecondary);
            var arrow = new Point[]
            {
                new(arrowX, arrowY),
                new(arrowX + 10, arrowY),
                new(arrowX + 5, arrowY + 5)
            };
            g.FillPolygon(arrowBrush, arrow);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var bgColor = isSelected ? AppTheme.SurfaceLight : AppTheme.SurfaceDark;

            using var bgBrush = new SolidBrush(bgColor);
            g.FillRectangle(bgBrush, e.Bounds);

            string text = Items[e.Index]?.ToString() ?? "";
            var textColor = isSelected ? AppTheme.TextPrimary : AppTheme.TextSecondary;
            using var textBrush = new SolidBrush(textColor);
            var textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 16, e.Bounds.Height);
            var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            g.DrawString(text, Font, textBrush, textRect, sf);
        }
    }
}
