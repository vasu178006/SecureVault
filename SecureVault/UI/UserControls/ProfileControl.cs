// ============================================
// SecureVault - Profile Control (Redesigned)
// Toast feedback, refined layout
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
        private string _userInitials = "?";

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
            _userInitials = GetInitials(user.FullName);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            Controls.Add(scroll);

            var root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2,
                BackColor = Color.Transparent, Padding = new Padding(24, 20, 24, 20)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scroll.Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "👤 My Profile", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 0, 0, 16)
            }, 0, 0);

            var card = new Panel { Size = new Size(520, 510), BackColor = Color.Transparent };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusCard, 10);
            root.Controls.Add(card, 0, 1);

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 10,
                BackColor = Color.Transparent, Padding = new Padding(28, 24, 28, 20)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(inner);

            // Profile picture + info row
            var profileRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3,
                BackColor = Color.Transparent
            };
            profileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            profileRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            profileRow.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            profileRow.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            profileRow.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

            _profilePic = new PictureBox
            {
                Size = new Size(80, 80), SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent, Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 12, 0)
            };
            bool hasImage = !string.IsNullOrEmpty(user.ProfileImagePath) && File.Exists(user.ProfileImagePath);
            if (hasImage)
            {
                _profilePic.Image = Image.FromFile(user.ProfileImagePath);
                _profileImagePath = user.ProfileImagePath;
            }
            SetupAvatarPaint(_profilePic);
            _profilePic.Click += ChangeProfilePicture;
            profileRow.SetRowSpan(_profilePic, 3);
            profileRow.Controls.Add(_profilePic, 0, 0);

            profileRow.Controls.Add(new Label
            {
                Text = user.FullName, Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 12, 0, 2)
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

            // Stats
            int docCount = 0; long storage = 0;
            try { docCount = _docService.GetDocumentCount(SessionManager.CurrentUserID); storage = _docService.GetStorageUsed(SessionManager.CurrentUserID); } catch { }
            var statsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            statsFlow.Controls.Add(new Label { Text = $"📂 {docCount} Documents", Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 8, 30, 0) });
            statsFlow.Controls.Add(new Label { Text = $"💾 {AnimationHelper.FormatBytes(storage)} Used", Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
            inner.Controls.Add(statsFlow, 0, 1);

            inner.Controls.Add(new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill }, 0, 2);
            inner.Controls.Add(MakeLabel("Full Name"), 0, 3);
            _nameBox = new RoundedTextBox { Text = user.FullName, Dock = DockStyle.Fill };
            inner.Controls.Add(_nameBox, 0, 4);
            inner.Controls.Add(MakeLabel("Email Address"), 0, 5);
            _emailBox = new RoundedTextBox { Text = user.Email, Dock = DockStyle.Fill };
            inner.Controls.Add(_emailBox, 0, 6);
            inner.Controls.Add(new Panel { BackColor = Color.Transparent, Dock = DockStyle.Fill }, 0, 7);

            var btnFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent };
            var saveBtn = new RoundedButton { Text = "💾 Save Changes", Size = new Size(160, 42), Margin = new Padding(0, 4, 10, 0) };
            saveBtn.Click += SaveProfile;
            btnFlow.Controls.Add(saveBtn);
            var passBtn = new RoundedButton { Text = "🔑 Change Password", Size = new Size(180, 42), IsGradient = false, FlatColor = AppTheme.SurfaceMid, Margin = new Padding(0, 4, 0, 0) };
            passBtn.Click += (s, e) => new ChangePasswordForm().ShowDialog();
            btnFlow.Controls.Add(passBtn);
            inner.Controls.Add(btnFlow, 0, 8);
        }

        private void SaveProfile(object? sender, EventArgs e)
        {
            var (success, msg) = _userService.UpdateProfile(
                SessionManager.CurrentUserID, _nameBox.Text.Trim(),
                _emailBox.Text.Trim(), _profileImagePath);
            if (success)
                ToastNotification.Show("Profile updated successfully!", ToastType.Success);
            else
                ToastNotification.Show(msg, ToastType.Error);
        }

        private void ChangeProfilePicture(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp", Title = "Select Profile Picture" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _profileImagePath = ofd.FileName;
                _profilePic.Image = Image.FromFile(ofd.FileName);
                _profilePic.Invalidate();
            }
        }

        private void SetupAvatarPaint(PictureBox pb)
        {
            pb.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                int w = pb.Width, h = pb.Height;
                using var circlePath = new GraphicsPath();
                circlePath.AddEllipse(0, 0, w - 1, h - 1);

                if (pb.Image != null)
                {
                    g.SetClip(circlePath);
                    g.DrawImage(pb.Image, 0, 0, w, h);
                    g.ResetClip();
                }
                else
                {
                    using var gradBrush = new LinearGradientBrush(new Rectangle(0, 0, w, h),
                        AppTheme.GradientStart, AppTheme.GradientEnd, 135f);
                    g.FillPath(gradBrush, circlePath);
                    using var initialsFont = new Font(AppTheme.FontFamily, 22, FontStyle.Bold);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(_userInitials, initialsFont, Brushes.White, new RectangleF(0, 0, w, h), sf);
                }

                // Gradient border ring
                using var borderPen = new Pen(Color.FromArgb(80, AppTheme.AccentGlow.R, AppTheme.AccentGlow.G, AppTheme.AccentGlow.B), 2f);
                g.DrawEllipse(borderPen, 1, 1, w - 3, h - 3);

                var bgColor = AppTheme.GetEffectiveBackColor(pb.Parent);
                using var bgBrush = new SolidBrush(bgColor);
                using var outerRegion = new Region(new Rectangle(0, 0, w, h));
                outerRegion.Exclude(circlePath);
                g.FillRegion(bgBrush, outerRegion);
            };
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
            return char.ToUpper(parts[0][0]).ToString();
        }

        private Label MakeLabel(string text) => new()
        {
            Text = text, Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
    }
}
