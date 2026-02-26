// ============================================
// SecureVault - Register Form (Redesigned)
// Password strength, validation feedback
// ============================================

using SecureVault.BLL;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.Forms
{
    public class RegisterForm : Form
    {
        private readonly AuthService _authService = new();
        private RoundedTextBox _nameBox = null!;
        private RoundedTextBox _emailBox = null!;
        private RoundedTextBox _passwordBox = null!;
        private RoundedTextBox _confirmBox = null!;
        private RoundedButton _registerButton = null!;
        private Label _errorLabel = null!;
        private ModernProgressBar _strengthBar = null!;
        private Label _strengthLabel = null!;

        public RegisterForm()
        {
            Text = "SecureVault – Register";
            Size = new Size(480, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;

            Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(ClientRectangle,
                    AppTheme.PrimaryDark, AppTheme.PrimaryMid, 90f);
                e.Graphics.FillRectangle(brush, ClientRectangle);
            };

            BuildUI();
            Load += (s, e) => AnimationHelper.FadeIn(this, 150);
        }

        private void BuildUI()
        {
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3,
                BackColor = Color.Transparent
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            Controls.Add(outer);

            var card = new Panel
            {
                Size = new Size(420, 590),
                BackColor = Color.Transparent
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusLarge);
            outer.Controls.Add(card, 1, 1);

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 14,
                BackColor = Color.Transparent, Padding = new Padding(30, 28, 30, 20)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // Title
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Name label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Name input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Email label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Email input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Pass label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Pass input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Strength bar + label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Confirm label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Confirm input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));   // Error
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // Button
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Fill
            card.Controls.Add(inner);

            int row = 0;
            inner.Controls.Add(new Label
            {
                Text = "Create Account",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, row++);

            inner.Controls.Add(MakeLabel("Full Name"), 0, row++);
            _nameBox = MakeInput("Enter your full name"); inner.Controls.Add(_nameBox, 0, row++);

            inner.Controls.Add(MakeLabel("Email Address"), 0, row++);
            _emailBox = MakeInput("Enter your email"); inner.Controls.Add(_emailBox, 0, row++);

            inner.Controls.Add(MakeLabel("Password"), 0, row++);
            _passwordBox = MakeInput("Min 6 chars, upper+lower+digit", true);
            _passwordBox.TextChanged += (s, e) => UpdatePasswordStrength();
            inner.Controls.Add(_passwordBox, 0, row++);

            // Password strength row (hidden until user types)
            var strengthRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                BackColor = Color.Transparent, Margin = new Padding(0),
                Visible = false
            };
            strengthRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            strengthRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            _strengthBar = new ModernProgressBar
            {
                Dock = DockStyle.Fill, Height = 6,
                BarColorStart = AppTheme.Error, BarColorEnd = AppTheme.Error,
                ShowShimmer = false, Margin = new Padding(0, 8, 8, 0)
            };
            _strengthLabel = new Label
            {
                Text = "", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            strengthRow.Controls.Add(_strengthBar, 0, 0);
            strengthRow.Controls.Add(_strengthLabel, 1, 0);
            inner.Controls.Add(strengthRow, 0, row++);

            inner.Controls.Add(MakeLabel("Confirm Password"), 0, row++);
            _confirmBox = MakeInput("Re-enter your password", true); inner.Controls.Add(_confirmBox, 0, row++);

            _errorLabel = new Label
            {
                Text = "", Font = AppTheme.BodySmall, ForeColor = AppTheme.Error,
                BackColor = Color.Transparent, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            inner.Controls.Add(_errorLabel, 0, row++);

            _registerButton = new RoundedButton
            {
                Text = "Create Account", Dock = DockStyle.Fill,
                ButtonColorStart = AppTheme.AccentTeal,
                ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186),
                HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 4, 0, 4)
            };
            _registerButton.Click += RegisterButton_Click;
            inner.Controls.Add(_registerButton, 0, row);

            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) RegisterButton_Click(this, EventArgs.Empty); };
        }

        private void UpdatePasswordStrength()
        {
            string pwd = _passwordBox.Text;
            int score = 0;
            if (pwd.Length >= 6) score++;
            if (pwd.Length >= 10) score++;
            if (pwd.Any(char.IsUpper)) score++;
            if (pwd.Any(char.IsLower)) score++;
            if (pwd.Any(char.IsDigit)) score++;
            if (pwd.Any(c => !char.IsLetterOrDigit(c))) score++;

            _strengthBar.Value = (int)(score / 6.0 * 100);
            if (score <= 2)
            {
                _strengthBar.BarColorStart = AppTheme.Error;
                _strengthBar.BarColorEnd = AppTheme.Error;
                _strengthLabel.Text = "Weak"; _strengthLabel.ForeColor = AppTheme.Error;
            }
            else if (score <= 4)
            {
                _strengthBar.BarColorStart = AppTheme.Warning;
                _strengthBar.BarColorEnd = Color.FromArgb(245, 158, 11);
                _strengthLabel.Text = "Medium"; _strengthLabel.ForeColor = AppTheme.Warning;
            }
            else
            {
                _strengthBar.BarColorStart = AppTheme.Success;
                _strengthBar.BarColorEnd = AppTheme.AccentTeal;
                _strengthLabel.Text = "Strong"; _strengthLabel.ForeColor = AppTheme.Success;
            }
            // Show/hide the strength row based on whether there's any password text
            if (_strengthBar.Parent?.Parent is Control strengthRow2)
                strengthRow2.Visible = !string.IsNullOrEmpty(pwd);
            if (string.IsNullOrEmpty(pwd)) { _strengthLabel.Text = ""; _strengthBar.Value = 0; }
        }

        private Label MakeLabel(string text) => new()
        {
            Text = text, Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };

        private RoundedTextBox MakeInput(string placeholder, bool isPassword = false) => new()
        {
            PlaceholderText = placeholder, IsPassword = isPassword,
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 4)
        };

        private void RegisterButton_Click(object? sender, EventArgs e)
        {
            _errorLabel.Text = "";
            _registerButton.Enabled = false;
            _registerButton.Text = "Creating...";
            try
            {
                var (success, message) = _authService.Register(
                    _nameBox.Text.Trim(), _emailBox.Text.Trim(),
                    _passwordBox.Text, _confirmBox.Text);
                if (success)
                {
                    ToastNotification.Show("Account created successfully!", ToastType.Success);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _errorLabel.Text = message;
                    AnimationHelper.ShakeControl(_errorLabel, 4, 300);
                }
            }
            catch (Exception ex) { _errorLabel.Text = $"Error: {ex.Message}"; }
            finally
            {
                _registerButton.Enabled = true;
                _registerButton.Text = "Create Account";
            }
        }
    }
}
