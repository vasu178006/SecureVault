// ============================================
// SecureVault - Styled DataGridView (Redesigned)
// Smooth row hover, refined styling,
// empty state support
// ============================================

using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium DataGridView with dark theme, smooth row hover,
    /// and empty-state message support.
    /// </summary>
    public class StyledDataGridView : DataGridView
    {
        public string EmptyMessage { get; set; } = "No items to display";

        public StyledDataGridView()
        {
            // General
            BackgroundColor = AppTheme.PrimaryDark;
            BorderStyle = BorderStyle.None;
            GridColor = Color.FromArgb(38, 38, 58);  // Must be opaque; approximates 30/255 white on dark bg
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ReadOnly = true;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            // Removed AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; so individual columns can dictate their fill/none behavior
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight = 48;
            RowTemplate.Height = 48;
            EnableHeadersVisualStyles = false;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            DoubleBuffered = true;
            Font = AppTheme.BodyRegular;

            // Header style
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.SurfaceDark,
                ForeColor = AppTheme.TextSecondary,
                Font = new Font(AppTheme.FontFamily, 10, FontStyle.Bold),
                SelectionBackColor = AppTheme.SurfaceDark,
                SelectionForeColor = AppTheme.TextSecondary,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Default cell style
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.PrimaryDark,
                ForeColor = AppTheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(40, AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B),
                SelectionForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyRegular,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Alternating rows
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.SurfaceDark,
                ForeColor = AppTheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(40, AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B),
                SelectionForeColor = AppTheme.TextPrimary
            };

            Scroll += (s, e) => Invalidate();
        }

        protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                Rows[e.RowIndex].DefaultCellStyle.BackColor = AppTheme.SurfaceLight;
            base.OnCellMouseEnter(e);
        }

        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    e.RowIndex % 2 == 0 ? AppTheme.PrimaryDark : AppTheme.SurfaceDark;
            base.OnCellMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Empty state overlay
            if (Rows.Count == 0 && !string.IsNullOrEmpty(EmptyMessage))
            {
                var g = e.Graphics;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                string icon = "📭";
                using var iconFont = new Font("Segoe UI Emoji", 28);
                var iconSize = TextRenderer.MeasureText(g, icon, iconFont);
                var textSize = TextRenderer.MeasureText(g, EmptyMessage, AppTheme.BodyLarge);

                int totalHeight = iconSize.Height + textSize.Height + 8;
                int startY = (Height - totalHeight) / 2;

                TextRenderer.DrawText(g, icon, iconFont,
                    new Rectangle(0, startY, Width, iconSize.Height),
                    AppTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, EmptyMessage, AppTheme.BodyLarge,
                    new Rectangle(0, startY + iconSize.Height + 8, Width, textSize.Height),
                    AppTheme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
