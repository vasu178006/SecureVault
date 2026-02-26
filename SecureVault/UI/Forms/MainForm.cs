// ============================================
// SecureVault - Main Form (Layout Refactored)
// Dock-based sidebar/topbar/content structure
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using SecureVault.UI.UserControls;

namespace SecureVault.UI.Forms
{
    public class MainForm : Form
    {
        private Panel _sidebarPanel = null!;
        private Panel _topBar = null!;
        private Panel _contentPanel = null!;
        private FlowLayoutPanel _sidebarFlow = null!;
        private Label _userNameLabel = null!;
        private Label _userRoleLabel = null!;
        private Label _pageTitleLabel = null!;

        private SidebarButton _btnDashboard = null!;
        private SidebarButton _btnDocuments = null!;
        private SidebarButton _btnUpload = null!;
        private SidebarButton _btnCategories = null!;
        private SidebarButton _btnRecycleBin = null!;
        private SidebarButton _btnActivityLog = null!;
        private SidebarButton _btnProfile = null!;
        private SidebarButton? _btnAdminDashboard;
        private SidebarButton? _btnUserManagement;
        private SidebarButton? _btnSystemLogs;
        private SidebarButton _btnLogout = null!;

        private const int SIDEBAR_WIDTH = 250;
        private const int TOPBAR_HEIGHT = 60;
        private UserControl? _currentControl;

        public MainForm()
        {
            InitializeForm();
            BuildLayout();
            NavigateTo("Dashboard");
            Load += (s, e) => AnimationHelper.FadeIn(this, 400);
        }

        private void InitializeForm()
        {
            Text = "SecureVault – Digital Locker";
            Size = new Size(1400, 850);
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font(AppTheme.FontFamily, 10);
        }

        private void BuildLayout()
        {
            // ── SIDEBAR: Dock.Left, fixed width ──
            _sidebarPanel = new Panel
            {
                Width = SIDEBAR_WIDTH,
                Dock = DockStyle.Left,
                BackColor = AppTheme.SidebarBg,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            _sidebarPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.SurfaceBorder, 1);
                e.Graphics.DrawLine(pen, _sidebarPanel.Width - 1, 0,
                    _sidebarPanel.Width - 1, _sidebarPanel.Height);
            };

            // Logo at top of sidebar (fixed)
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = AppTheme.SidebarBg
            };
            var logoLabel = new Label
            {
                Text = "🔒 SecureVault",
                Font = new Font(AppTheme.FontFamily, 16, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.SidebarBg,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoPanel.Controls.Add(logoLabel);

            // Separator
            var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.SurfaceBorder };

            // FlowLayoutPanel for menu buttons
            _sidebarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = AppTheme.SidebarBg,
                Padding = new Padding(8, 10, 8, 10)
            };

            // Section label
            _sidebarFlow.Controls.Add(MakeSectionLabel("MAIN", AppTheme.TextMuted));

            _btnDashboard = MakeSidebarBtn("Dashboard", "📊");
            _btnDocuments = MakeSidebarBtn("My Documents", "📁");
            _btnUpload = MakeSidebarBtn("Upload Files", "📤");
            _btnCategories = MakeSidebarBtn("Categories", "🏷️");
            _btnRecycleBin = MakeSidebarBtn("Recycle Bin", "🗑️");
            _btnActivityLog = MakeSidebarBtn("Activity Log", "📋");
            _btnProfile = MakeSidebarBtn("My Profile", "👤");

            if (SessionManager.IsAdmin)
            {
                _sidebarFlow.Controls.Add(new Panel { Height = 8, Width = SIDEBAR_WIDTH - 20, BackColor = AppTheme.SidebarBg });
                _sidebarFlow.Controls.Add(new Panel { Height = 1, Width = SIDEBAR_WIDTH - 40, BackColor = AppTheme.SurfaceBorder, Margin = new Padding(10, 0, 10, 0) });
                _sidebarFlow.Controls.Add(MakeSectionLabel("ADMIN", AppTheme.AccentPink));
                _btnAdminDashboard = MakeSidebarBtn("Admin Dashboard", "⚡");
                _btnUserManagement = MakeSidebarBtn("Manage Users", "👥");
                _btnSystemLogs = MakeSidebarBtn("System Logs", "🔍");
            }

            // Logout at bottom
            var logoutPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 8)
            };
            _btnLogout = new SidebarButton
            {
                Text = "Logout",
                IconText = "🚪",
                Dock = DockStyle.Fill
            };
            _btnLogout.Click += (s, e) => PerformLogout();
            logoutPanel.Controls.Add(_btnLogout);

            // Assemble sidebar (order matters for Dock: bottom first, then tops, then fill last)
            _sidebarPanel.Controls.Add(logoutPanel);     // Bottom
            _sidebarPanel.Controls.Add(logoPanel);       // Top
            _sidebarPanel.Controls.Add(sep);             // Top (after logo)
            _sidebarPanel.Controls.Add(_sidebarFlow);   // Fill

            // ── TOPBAR: Dock.Top, fixed height ──
            _topBar = new Panel
            {
                Height = TOPBAR_HEIGHT,
                Dock = DockStyle.Top,
                BackColor = AppTheme.PrimaryDark,
                Padding = new Padding(20, 0, 20, 0),
                Margin = new Padding(0)
            };
            _topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.SurfaceBorder, 1);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };

            _pageTitleLabel = new Label
            {
                Text = "Dashboard",
                Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 300,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _topBar.Controls.Add(_pageTitleLabel);

            var userInfoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            userInfoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            userInfoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            _userNameLabel = new Label
            {
                Text = SessionManager.CurrentUser?.FullName ?? "User",
                Font = new Font(AppTheme.FontFamily, 11, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Margin = new Padding(0, 0, 0, 0)
            };
            _userRoleLabel = new Label
            {
                Text = SessionManager.CurrentUser?.Role ?? "User",
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Margin = new Padding(0, 2, 0, 0)
            };
            userInfoPanel.Controls.Add(_userNameLabel, 0, 0);
            userInfoPanel.Controls.Add(_userRoleLabel, 0, 1);
            _topBar.Controls.Add(userInfoPanel);

            // ── CONTENT: Dock.Fill ──
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.PrimaryDark,
                Padding = new Padding(0)
            };

            // Dock order: sidebar added last = highest priority = full height left.
            // TopBar fills width to right of sidebar. Content fills remaining.
            Controls.Add(_contentPanel);
            Controls.Add(_topBar);
            Controls.Add(_sidebarPanel);
        }

        private SidebarButton MakeSidebarBtn(string text, string icon)
        {
            var btn = new SidebarButton
            {
                Text = text,
                IconText = icon,
                Width = SIDEBAR_WIDTH - 20,
                Height = 46,
                Margin = new Padding(0, 2, 0, 2)
            };
            btn.Click += (s, e) => NavigateTo(text);
            _sidebarFlow.Controls.Add(btn);
            return btn;
        }

        private Label MakeSectionLabel(string text, Color color)
        {
            return new Label
            {
                Text = $"  {text}",
                Font = new Font(AppTheme.FontFamily, 8, FontStyle.Bold),
                ForeColor = color,
                BackColor = AppTheme.SidebarBg,
                AutoSize = false,
                Width = SIDEBAR_WIDTH - 20,
                Height = 25,
                Margin = new Padding(0, 6, 0, 4),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void NavigateTo(string section)
        {
            SetAllButtonsInactive();
            _pageTitleLabel.Text = section;

            if (_currentControl != null)
            {
                _contentPanel.Controls.Remove(_currentControl);
                _currentControl.Dispose();
            }

            UserControl newControl = section switch
            {
                "Dashboard" => SetActive(_btnDashboard, new DashboardControl()),
                "My Documents" => SetActive(_btnDocuments, new DocumentListControl()),
                "Upload Files" => SetActive(_btnUpload, new UploadControl()),
                "Categories" => SetActive(_btnCategories, new CategoryControl()),
                "Recycle Bin" => SetActive(_btnRecycleBin, new RecycleBinControl()),
                "Activity Log" => SetActive(_btnActivityLog, new ActivityLogControl()),
                "My Profile" => SetActive(_btnProfile, new ProfileControl()),
                "Admin Dashboard" => SetActive(_btnAdminDashboard, new AdminDashboardControl()),
                "Manage Users" => SetActive(_btnUserManagement, new UserManagementControl()),
                "System Logs" => SetActive(_btnSystemLogs, new SystemLogControl()),
                _ => SetActive(_btnDashboard, new DashboardControl())
            };

            newControl.Dock = DockStyle.Fill;
            newControl.BackColor = AppTheme.PrimaryDark;
            _contentPanel.Controls.Add(newControl);
            _currentControl = newControl;
        }

        private UserControl SetActive(SidebarButton? btn, UserControl control)
        {
            if (btn != null) btn.IsActive = true;
            return control;
        }

        private void SetAllButtonsInactive()
        {
            _btnDashboard.IsActive = false;
            _btnDocuments.IsActive = false;
            _btnUpload.IsActive = false;
            _btnCategories.IsActive = false;
            _btnRecycleBin.IsActive = false;
            _btnActivityLog.IsActive = false;
            _btnProfile.IsActive = false;
            if (_btnAdminDashboard != null) _btnAdminDashboard.IsActive = false;
            if (_btnUserManagement != null) _btnUserManagement.IsActive = false;
            if (_btnSystemLogs != null) _btnSystemLogs.IsActive = false;
        }

        private void PerformLogout()
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                new AuthService().Logout();
                var loginForm = new LoginForm();
                loginForm.Show();
                Close();
            }
        }
    }
}
