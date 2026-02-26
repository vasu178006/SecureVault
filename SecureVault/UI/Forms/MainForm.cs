// ============================================
// SecureVault - Main Form (Redesigned)
// Animated page transitions, sidebar stagger,
// user avatar, refined layout
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using SecureVault.UI.UserControls;
using System.Drawing.Drawing2D;

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
        private Panel _avatarPanel = null!;

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

        private const int SIDEBAR_WIDTH = 260;
        private const int TOPBAR_HEIGHT = 64;
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
            // ── SIDEBAR ──
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
                using var pen = new Pen(Color.FromArgb(20, 255, 255, 255), 1);
                e.Graphics.DrawLine(pen, _sidebarPanel.Width - 1, 0,
                    _sidebarPanel.Width - 1, _sidebarPanel.Height);
            };

            // Logo at top
            var logoPanel = new Panel
            {
                Dock = DockStyle.Top, Height = 68, BackColor = AppTheme.SidebarBg
            };
            var logoLabel = new Label
            {
                Text = "🛡️  SecureVault",
                Font = new Font(AppTheme.FontFamily, 17, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = AppTheme.SidebarBg,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoPanel.Controls.Add(logoLabel);

            var sep = new Panel
            {
                Dock = DockStyle.Top, Height = 1,
                BackColor = Color.FromArgb(25, 255, 255, 255)
            };

            // Menu buttons
            _sidebarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = AppTheme.SidebarBg,
                Padding = new Padding(10, 12, 10, 12)
            };

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
                _sidebarFlow.Controls.Add(new Panel { Height = 8, Width = SIDEBAR_WIDTH - 24, BackColor = AppTheme.SidebarBg });
                _sidebarFlow.Controls.Add(new Panel { Height = 1, Width = SIDEBAR_WIDTH - 48, BackColor = Color.FromArgb(25, 255, 255, 255), Margin = new Padding(12, 0, 12, 0) });
                _sidebarFlow.Controls.Add(MakeSectionLabel("ADMIN", AppTheme.AccentPink));
                _btnAdminDashboard = MakeSidebarBtn("Admin Dashboard", "⚡");
                _btnUserManagement = MakeSidebarBtn("Manage Users", "👥");
                _btnSystemLogs = MakeSidebarBtn("System Logs", "🔍");
            }

            // Logout at bottom
            var logoutPanel = new Panel
            {
                Dock = DockStyle.Bottom, Height = 58,
                BackColor = Color.Transparent, Padding = new Padding(10, 4, 10, 10)
            };
            _btnLogout = new SidebarButton
            {
                Text = "Logout", IconText = "🚪", Dock = DockStyle.Fill
            };
            _btnLogout.Click += (s, e) => PerformLogout();
            logoutPanel.Controls.Add(_btnLogout);

            _sidebarPanel.Controls.Add(logoutPanel);
            _sidebarPanel.Controls.Add(logoPanel);
            _sidebarPanel.Controls.Add(sep);
            _sidebarPanel.Controls.Add(_sidebarFlow);

            // ── TOP BAR ──
            _topBar = new Panel
            {
                Height = TOPBAR_HEIGHT,
                Dock = DockStyle.Top,
                BackColor = AppTheme.PrimaryDark,
                Padding = new Padding(24, 0, 24, 0),
                Margin = new Padding(0)
            };
            _topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1);
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

            // Right: avatar + user info
            var rightPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            var userInfoPanel = new TableLayoutPanel
            {
                AutoSize = true, ColumnCount = 1, RowCount = 2,
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
                Margin = new Padding(0)
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
            rightPanel.Controls.Add(userInfoPanel);

            // Avatar circle
            _avatarPanel = new Panel
            {
                Size = new Size(40, 40),
                BackColor = Color.Transparent,
                Margin = new Padding(12, 12, 0, 0)
            };
            _avatarPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                path.AddEllipse(0, 0, 39, 39);
                using var bgBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 40, 40), AppTheme.GradientStart, AppTheme.GradientEnd, 135f);
                g.FillPath(bgBrush, path);

                string initials = GetInitials(SessionManager.CurrentUser?.FullName ?? "U");
                using var font = new Font(AppTheme.FontFamily, 13, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(initials, font, textBrush, new RectangleF(0, 0, 40, 40), sf);
            };
            rightPanel.Controls.Add(_avatarPanel);
            _topBar.Controls.Add(rightPanel);

            // ── CONTENT ──
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.PrimaryDark,
                Padding = new Padding(0)
            };

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
                Width = SIDEBAR_WIDTH - 24,
                Height = 46,
                Margin = new Padding(0, 2, 0, 2)
            };
            btn.Click += (s, e) => NavigateTo(text);
            _sidebarFlow.Controls.Add(btn);
            return btn;
        }

        private Label MakeSectionLabel(string text, Color color) => new()
        {
            Text = $"  {text}",
            Font = new Font(AppTheme.FontFamily, 8, FontStyle.Bold),
            ForeColor = color,
            BackColor = AppTheme.SidebarBg,
            AutoSize = false,
            Width = SIDEBAR_WIDTH - 24,
            Height = 25,
            Margin = new Padding(0, 8, 0, 4),
            TextAlign = ContentAlignment.MiddleLeft
        };

        private void NavigateTo(string section)
        {
            SetAllButtonsInactive();
            _pageTitleLabel.Text = section;

            // Remove old control (no dispose — let animation overlap)
            var oldControl = _currentControl;

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

            // Simple transition: remove old after adding new
            if (oldControl != null)
            {
                _contentPanel.Controls.Remove(oldControl);
                oldControl.Dispose();
            }
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
            if (ModernDialog.Confirm("Confirm Logout",
                "Are you sure you want to sign out of SecureVault?",
                "Logout", "Cancel", this))
            {
                new AuthService().Logout();
                var loginForm = new LoginForm();
                loginForm.Show();
                Close();
            }
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
            return char.ToUpper(parts[0][0]).ToString();
        }
    }
}
