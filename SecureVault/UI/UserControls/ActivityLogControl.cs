// ============================================
// SecureVault - Activity Log (Redesigned)
// Consistent spacing, empty state
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class ActivityLogControl : UserControl
    {
        private readonly ActivityLogService _logService = new();
        private StyledDataGridView _grid = null!;
        private DateTimePicker _dateFrom = null!;
        private DateTimePicker _dateTo = null!;

        public ActivityLogControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 14)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "📋 Activity Log", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            filterFlow.Controls.Add(new Label { Text = "From:", Font = AppTheme.BodyRegular, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 14, 6, 0) });
            _dateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1), Size = new Size(130, 32), Margin = new Padding(0, 10, 12, 0) };
            filterFlow.Controls.Add(_dateFrom);
            filterFlow.Controls.Add(new Label { Text = "To:", Font = AppTheme.BodyRegular, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 14, 6, 0) });
            _dateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now, Size = new Size(130, 32), Margin = new Padding(0, 10, 12, 0) };
            filterFlow.Controls.Add(_dateTo);
            var filterBtn = new RoundedButton { Text = "🔍 Filter", Size = new Size(100, 38), IsGradient = false, FlatColor = AppTheme.SurfaceMid, CornerRadius = 10, Margin = new Padding(0, 8, 0, 0) };
            filterBtn.Click += (s, e) => LoadLogs();
            filterFlow.Controls.Add(filterBtn);
            root.Controls.Add(filterFlow, 0, 1);

            _grid = new StyledDataGridView
            {
                Dock = DockStyle.Fill,
                EmptyMessage = "No activity logs found for this period"
            };
            _grid.Columns.Add("Timestamp", "Date & Time");
            _grid.Columns["Timestamp"].Width = 170; _grid.Columns["Timestamp"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Action", "Action");
            _grid.Columns["Action"].Width = 150; _grid.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Description", "Description");
            root.Controls.Add(_grid, 0, 2);

            LoadLogs();
        }

        private void LoadLogs()
        {
            _grid.Rows.Clear();
            try
            {
                foreach (var log in _logService.GetUserLogs(SessionManager.CurrentUserID, _dateFrom.Value.Date, _dateTo.Value.Date))
                    _grid.Rows.Add(log.Timestamp.ToString("MMM dd, yyyy HH:mm"), log.Action, log.Description ?? "—");
            }
            catch { }
        }
    }
}
