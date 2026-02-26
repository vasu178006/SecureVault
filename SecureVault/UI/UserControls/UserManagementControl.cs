// ============================================
// SecureVault - User Management (Redesigned)
// Toast/ModernDialog, consistent spacing
// ============================================

using SecureVault.BLL;
using SecureVault.Models;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class UserManagementControl : UserControl
    {
        private readonly UserService _userService = new();
        private StyledDataGridView _grid = null!;
        private RoundedTextBox _searchBox = null!;
        private List<User> _users = new();

        public UserManagementControl()
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "👥 User Management", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var toolFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            _searchBox = new RoundedTextBox
            {
                PlaceholderText = "🔍 Search users...", Size = new Size(300, 42),
                Margin = new Padding(0, 6, 10, 0)
            };
            _searchBox.TextChanged += (s, e) => LoadUsers();
            toolFlow.Controls.Add(_searchBox);

            var blockBtn = new RoundedButton
            {
                Text = "🔒 Block/Unblock", Size = new Size(160, 40),
                IsGradient = false, FlatColor = AppTheme.Warning, ForeColor = Color.Black,
                CornerRadius = 10, Margin = new Padding(0, 7, 10, 0)
            };
            blockBtn.Click += ToggleBlock;
            toolFlow.Controls.Add(blockBtn);

            var delBtn = new RoundedButton
            {
                Text = "❌ Delete User", Size = new Size(140, 40),
                ButtonColorStart = AppTheme.Error, ButtonColorEnd = Color.FromArgb(185, 28, 28),
                HoverColorStart = Color.FromArgb(255, 88, 88), HoverColorEnd = Color.FromArgb(205, 48, 48),
                CornerRadius = 10, Margin = new Padding(0, 7, 0, 0)
            };
            delBtn.Click += DeleteUser;
            toolFlow.Controls.Add(delBtn);
            root.Controls.Add(toolFlow, 0, 1);

            _grid = new StyledDataGridView
            {
                Dock = DockStyle.Fill,
                EmptyMessage = "No users found"
            };
            _grid.Columns.Add("ID", "ID"); _grid.Columns["ID"].Width = 50; _grid.Columns["ID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Name", "Full Name");
            _grid.Columns.Add("Email", "Email");
            _grid.Columns.Add("Role", "Role"); _grid.Columns["Role"].Width = 70; _grid.Columns["Role"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Status", "Status"); _grid.Columns["Status"].Width = 100; _grid.Columns["Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("LastLogin", "Last Login"); _grid.Columns["LastLogin"].Width = 150; _grid.Columns["LastLogin"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Created", "Created"); _grid.Columns["Created"].Width = 130; _grid.Columns["Created"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            root.Controls.Add(_grid, 0, 2);

            LoadUsers();
        }

        private void LoadUsers()
        {
            _grid.Rows.Clear();
            try
            {
                string? search = string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text;
                _users = _userService.GetAll(search);
                foreach (var u in _users)
                {
                    int ri = _grid.Rows.Add(u.UserID, u.FullName, u.Email, u.Role,
                        u.IsBlocked ? "🔒 Blocked" : "✅ Active",
                        u.LastLogin?.ToString("MMM dd, yyyy HH:mm") ?? "Never",
                        u.CreatedAt.ToString("MMM dd, yyyy"));
                    if (u.IsBlocked) _grid.Rows[ri].DefaultCellStyle.ForeColor = AppTheme.Error;
                }
            }
            catch { }
        }

        private User? GetSel() =>
            _grid.SelectedRows.Count > 0 && _grid.SelectedRows[0].Index < _users.Count
                ? _users[_grid.SelectedRows[0].Index] : null;

        private void ToggleBlock(object? sender, EventArgs e)
        {
            var u = GetSel(); if (u == null) return;
            if (u.IsBlocked)
            {
                _userService.UnblockUser(u.UserID);
                LoadUsers();
                ToastNotification.Show($"'{u.FullName}' has been unblocked", ToastType.Success);
            }
            else if (ModernDialog.Confirm("Block User",
                $"Block '{u.FullName}'? They won't be able to log in.",
                "Block", "Cancel", FindForm()))
            {
                _userService.BlockUser(u.UserID);
                LoadUsers();
                ToastNotification.Show($"'{u.FullName}' has been blocked", ToastType.Warning);
            }
        }

        private void DeleteUser(object? sender, EventArgs e)
        {
            var u = GetSel(); if (u == null) return;
            if (ModernDialog.ConfirmDelete("Delete User",
                $"Permanently delete '{u.FullName}'?\nAll their documents will also be deleted!",
                "Delete User", FindForm()))
            {
                _userService.DeleteUser(u.UserID);
                LoadUsers();
                ToastNotification.Show("User deleted", ToastType.Info);
            }
        }
    }
}
