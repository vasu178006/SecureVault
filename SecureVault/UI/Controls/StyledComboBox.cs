// ============================================
// SecureVault - Styled ComboBox (Redesigned)
// Smooth hover, animated arrow, refined styling
// ============================================

using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// Owner-drawn ComboBox with smooth hover transitions and dark theme.
    /// </summary>
    public class StyledComboBox : ComboBox
    {
        private float _hoverProgress;
        private System.Windows.Forms.Timer? _hoverTimer;

        public StyledComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            BackColor = AppTheme.SurfaceDark;
            ForeColor = AppTheme.TextPrimary;
            FlatStyle = FlatStyle.Flat;
            Font = AppTheme.BodyRegular;
            ItemHeight = 30;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            StartHoverAnimation(1f);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
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
                if ((step > 0 && _hoverProgress >= target) || (step < 0 && _hoverProgress <= target))
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Background (smooth lerp)
            var bgColor = AppTheme.LerpColor(AppTheme.SurfaceDark, AppTheme.SurfaceMid, _hoverProgress);
            using var bgPath = AppTheme.CreateRoundedRect(rect, 8);
            using var bgBrush = new SolidBrush(bgColor);
            g.FillPath(bgBrush, bgPath);

            // Border (smooth lerp)
            var borderColor = AppTheme.LerpColor(
                AppTheme.SurfaceBorder,
                Color.FromArgb(80, AppTheme.AccentGlow.R, AppTheme.AccentGlow.G, AppTheme.AccentGlow.B),
                _hoverProgress);
            using var borderPen = new Pen(borderColor, 1f);
            g.DrawPath(borderPen, bgPath);

            // Selected text
            string text = SelectedItem?.ToString() ?? "";
            using var textBrush = new SolidBrush(AppTheme.TextPrimary);
            var textRect = new Rectangle(12, 0, Width - 36, Height);
            var sf = new StringFormat { 
                LineAlignment = StringAlignment.Center, 
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(text, Font, textBrush, textRect, sf);

            // Dropdown chevron
            int arrowX = Width - 22;
            int arrowY = Height / 2 - 2;
            using var arrowPen = new Pen(AppTheme.LerpColor(AppTheme.TextMuted, AppTheme.TextSecondary, _hoverProgress), 1.5f);
            g.DrawLine(arrowPen, arrowX, arrowY, arrowX + 5, arrowY + 4);
            g.DrawLine(arrowPen, arrowX + 5, arrowY + 4, arrowX + 10, arrowY);
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
            var textRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 20, e.Bounds.Height);
            var sf = new StringFormat { 
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(text, Font, textBrush, textRect, sf);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverTimer?.Stop();
                _hoverTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
