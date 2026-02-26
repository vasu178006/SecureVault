// ============================================
// SecureVault - Styled DataGridView
// Modern dark-themed DataGridView
// ============================================

using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A DataGridView styled with the dark theme.
    /// </summary>
    public class StyledDataGridView : DataGridView
    {
        public StyledDataGridView()
        {
            // General
            BackgroundColor = AppTheme.PrimaryDark;
            BorderStyle = BorderStyle.None;
            GridColor = AppTheme.SurfaceBorder;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ReadOnly = true;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight = 45;
            RowTemplate.Height = 42;
            EnableHeadersVisualStyles = false;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            DoubleBuffered = true;
            Font = AppTheme.BodyRegular;

            // Header style
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.SurfaceMid,
                ForeColor = AppTheme.TextSecondary,
                Font = new Font(AppTheme.FontFamily, 10, FontStyle.Bold),
                SelectionBackColor = AppTheme.SurfaceMid,
                SelectionForeColor = AppTheme.TextSecondary,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            // Default cell style
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.PrimaryDark,
                ForeColor = AppTheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(50, AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B),
                SelectionForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyRegular,
                Padding = new Padding(8, 0, 0, 0)
            };

            // Alternating rows
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.SurfaceDark,
                ForeColor = AppTheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(50, AppTheme.GradientStart.R, AppTheme.GradientStart.G, AppTheme.GradientStart.B),
                SelectionForeColor = AppTheme.TextPrimary
            };

            // Scrollbar styling via event
            Scroll += (s, e) => Invalidate();
        }

        protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Rows[e.RowIndex].DefaultCellStyle.BackColor = AppTheme.SurfaceLight;
            }
            base.OnCellMouseEnter(e);
        }

        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    e.RowIndex % 2 == 0 ? AppTheme.PrimaryDark : AppTheme.SurfaceDark;
            }
            base.OnCellMouseLeave(e);
        }
    }
}
