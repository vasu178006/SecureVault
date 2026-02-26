// ============================================
// SecureVault - Profile Control (Layout Refactored)
// TableLayoutPanel-based profile card
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Forms;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.UserControls
{
    public class ProfileControl : UserControl
    {
        private readonly UserService _userService = new();
        private readonly DocumentService _docService = new();
        private RoundedTextBox _nameBox = null!;
        private RoundedTextBox _emailBox = null!;
        private PictureBox _profilePic = null!;
        private string? _profileImagePath;

        public ProfileControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();
            var user = SessionManager.CurrentUser;
            if (user == null) return;

            // Root scrollable
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            Controls.Add(scroll);

            var root = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 15, 20, 15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scroll.Controls.Add(root);

            // Title
            root.Controls.Add(new Label
            {
                Text = "👤 My Profile",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            }, 0, 0);

            // Profile card
            var card = new Panel
            {
                Size = new Size(520, 480),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusMedium);
            root.Controls.Add(card, 0, 1);

            // Card inner layout
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                BackColor = Color.Transparent,
                Padding = new Padding(25, 20, 25, 15)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Profile row (pic + info)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Stats row
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));  // Spacer
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Name label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Name input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Email label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Email input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));  // Spacer
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Buttons
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Fill
            card.Controls.Add(inner);

            // Row 0: Profile pic + info
            var profileRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            profileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            profileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _profilePic = new PictureBox
            {
                Size = new Size(75, 75),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = AppTheme.SurfaceMid,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 10, 0)
            };
            MakeCircular(_profilePic);
            if (!string.IsNullOrEmpty(user.ProfileImagePath) && File.Exists(user.ProfileImagePath))
            {
                _profilePic.Image = Image.FromFile(user.ProfileImagePath);
                _profileImagePath = user.ProfileImagePath;
            }
            _profilePic.Click += ChangeProfilePicture;
            profileRow.SetRowSpan(_profilePic, 3);
            profileRow.Controls.Add(_profilePic, 0, 0);

            profileRow.Controls.Add(new Label
            {
                Text = user.FullName, Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 10, 0, 0)
            }, 1, 0);
            profileRow.Controls.Add(new Label
            {
                Text = user.Role, Font = AppTheme.BodyRegular,
                ForeColor = user.IsAdmin ? AppTheme.AccentPink : AppTheme.AccentTeal,
                BackColor = Color.Transparent, AutoSize = true
            }, 1, 1);
            profileRow.Controls.Add(new Label
            {
                Text = $"Member since {user.CreatedAt:MMMM yyyy}",
                Font = AppTheme.BodySmall, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent, AutoSize = true
            }, 1, 2);
            inner.Controls.Add(profileRow, 0, 0);

            // Row 1: Stats
            int docCount = 0; long storage = 0;
            try { docCount = _docService.GetDocumentCount(SessionManager.CurrentUserID); storage = _docService.GetStorageUsed(SessionManager.CurrentUserID); } catch { }

            var statsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            statsFlow.Controls.Add(new Label
            {
                Text = $"📂 {docCount} Documents", Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 5, 30, 0)
            });
            statsFlow.Controls.Add(new Label
            {
                Text = $"💾 {AnimationHelper.FormatBytes(storage)} Used", Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 5, 0, 0)
            });
            inner.Controls.Add(statsFlow, 0, 1);

            // Row 2: spacer
            inner.Controls.Add(new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill }, 0, 2);

            // Row 3-4: Name
            inner.Controls.Add(MakeLabel("Full Name"), 0, 3);
            _nameBox = new RoundedTextBox { Text = user.FullName, Dock = DockStyle.Fill };
            inner.Controls.Add(_nameBox, 0, 4);

            // Row 5-6: Email
            inner.Controls.Add(MakeLabel("Email Address"), 0, 5);
            _emailBox = new RoundedTextBox { Text = user.Email, Dock = DockStyle.Fill };
            inner.Controls.Add(_emailBox, 0, 6);

            // Row 7: spacer
            inner.Controls.Add(new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill }, 0, 7);

            // Row 8: Buttons
            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var saveBtn = new RoundedButton { Text = "💾 Save Changes", Size = new Size(160, 42), Margin = new Padding(0, 4, 10, 0) };
            saveBtn.Click += SaveProfile;
            btnFlow.Controls.Add(saveBtn);

            var passBtn = new RoundedButton
            {
                Text = "🔑 Change Password", Size = new Size(180, 42),
                IsGradient = false, FlatColor = AppTheme.SurfaceMid, Margin = new Padding(0, 4, 0, 0)
            };
            passBtn.Click += (s, e) => new ChangePasswordForm().ShowDialog();
            btnFlow.Controls.Add(passBtn);
            inner.Controls.Add(btnFlow, 0, 8);
        }

        private void SaveProfile(object? sender, EventArgs e)
        {
            var (success, msg) = _userService.UpdateProfile(
                SessionManager.CurrentUserID, _nameBox.Text.Trim(),
                _emailBox.Text.Trim(), _profileImagePath);
            MessageBox.Show(msg, success ? "Success" : "Error", MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void ChangeProfilePicture(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp", Title = "Select Profile Picture" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _profileImagePath = ofd.FileName;
                _profilePic.Image = Image.FromFile(ofd.FileName);
            }
        }

        private void MakeCircular(PictureBox pb)
        {
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, pb.Width, pb.Height);
            pb.Region = new Region(path);
        }

        private Label MakeLabel(string text) => new()
        {
            Text = text, Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
    }
}
