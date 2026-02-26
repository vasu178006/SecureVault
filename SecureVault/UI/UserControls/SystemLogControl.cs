// ============================================
// SecureVault - System Log (Layout Refactored)
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class SystemLogControl : UserControl
    {
        private readonly ActivityLogService _logService = new();
        private readonly UserService _userService = new();
        private StyledDataGridView _grid = null!;
        private StyledComboBox _userFilter = null!;
        private DateTimePicker _dateFrom = null!;
        private DateTimePicker _dateTo = null!;
        private List<Models.User> _allUsers = new();

        public SystemLogControl()
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
                BackColor = Color.Transparent, Padding = new Padding(15, 10, 15, 10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "🔍 System Logs", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            filterFlow.Controls.Add(new Label { Text = "User:", Font = AppTheme.BodyRegular, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 12, 5, 0) });
            _userFilter = new StyledComboBox
            {
                Size = new Size(200, 30), Margin = new Padding(0, 8, 10, 0)
            };
            _userFilter.Items.Add("All Users");
            try { _allUsers = _userService.GetAll(); foreach (var u in _allUsers) _userFilter.Items.Add($"{u.FullName} ({u.Email})"); } catch { }
            _userFilter.SelectedIndex = 0;
            filterFlow.Controls.Add(_userFilter);

            filterFlow.Controls.Add(new Label { Text = "From:", Font = AppTheme.BodyRegular, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 12, 5, 0) });
            _dateFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddMonths(-1), Size = new Size(130, 30), Margin = new Padding(0, 8, 10, 0) };
            filterFlow.Controls.Add(_dateFrom);
            filterFlow.Controls.Add(new Label { Text = "To:", Font = AppTheme.BodyRegular, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 12, 5, 0) });
            _dateTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Now, Size = new Size(130, 30), Margin = new Padding(0, 8, 10, 0) };
            filterFlow.Controls.Add(_dateTo);

            var filterBtn = new RoundedButton { Text = "🔍 Filter", Size = new Size(100, 36), IsGradient = false, FlatColor = AppTheme.SurfaceMid, CornerRadius = 8, Margin = new Padding(0, 6, 0, 0) };
            filterBtn.Click += (s, e) => LoadLogs();
            filterFlow.Controls.Add(filterBtn);
            root.Controls.Add(filterFlow, 0, 1);

            _grid = new StyledDataGridView { Dock = DockStyle.Fill };
            _grid.Columns.Add("Timestamp", "Date & Time"); _grid.Columns["Timestamp"].Width = 170; _grid.Columns["Timestamp"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("User", "User"); _grid.Columns["User"].Width = 160; _grid.Columns["User"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Action", "Action"); _grid.Columns["Action"].Width = 150; _grid.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Description", "Description");
            root.Controls.Add(_grid, 0, 2);

            LoadLogs();
        }

        private void LoadLogs()
        {
            _grid.Rows.Clear();
            try
            {
                int? userId = _userFilter.SelectedIndex > 0 ? _allUsers[_userFilter.SelectedIndex - 1].UserID : null;
                foreach (var log in _logService.GetAllLogs(userId, _dateFrom.Value.Date, _dateTo.Value.Date))
                    _grid.Rows.Add(log.Timestamp.ToString("MMM dd, yyyy HH:mm:ss"), log.UserName ?? "Unknown", log.Action, log.Description ?? "—");
            }
            catch { }
        }
    }
}
