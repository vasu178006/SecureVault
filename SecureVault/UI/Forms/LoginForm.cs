// ============================================
// SecureVault - Login Form (Redesigned)
// Animated background, glass card scale-in,
// floating-label inputs, loading state
// ============================================

using SecureVault.BLL;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService = new();
        private RoundedTextBox _emailBox = null!;
        private RoundedTextBox _passwordBox = null!;
        private RoundedButton _loginButton = null!;
        private Label _errorLabel = null!;
        private LinkLabel _registerLink = null!;


        public LoginForm()
        {
            Text = "SecureVault – Login";
            Size = new Size(480, 620);
            StartPosition = FormStartPosition.CenterScreen;
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
            // Outer centering container
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            Controls.Add(outer);

            // Glass card panel (matched to RegisterForm sizing)
            var card = new Panel
            {
                Size = new Size(420, 500),
                BackColor = Color.Transparent
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusLarge);
            outer.Controls.Add(card, 1, 1);

            // Inner layout inside card
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 11,
                BackColor = Color.Transparent,
                Padding = new Padding(30, 28, 30, 20)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Shield icon (was 48)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // Title (was 40)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));   // Subtitle (was 30 - adds big gap before email)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));   // Email label (was 22)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Email input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));   // Pass label (match email)
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Pass input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));   // Error label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // Login button
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // Register link
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Fill
            card.Controls.Add(inner);

            int row = 0;

            // Row 0: Shield icon
            inner.Controls.Add(new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 22),
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, row++);

            // Row 1: Title
            inner.Controls.Add(new Label
            {
                Text = "SecureVault",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, row++);

            // Row 2: Subtitle
            inner.Controls.Add(new Label
            {
                Text = "Sign in to your digital locker",
                Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.TextSecondary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            }, 0, row++);

            // Row 3: Email label
            inner.Controls.Add(MakeLabel("Email Address"), 0, row++);

            // Row 4: Email input
            _emailBox = MakeInput("Enter your email");
            inner.Controls.Add(_emailBox, 0, row++);

            // Row 5: Password label
            inner.Controls.Add(MakeLabel("Password"), 0, row++);

            // Row 6: Password input
            _passwordBox = MakeInput("Enter your password", true);
            inner.Controls.Add(_passwordBox, 0, row++);

            // Row 7: Error label
            _errorLabel = new Label
            {
                Text = "",
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.Error,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = true,
                MaximumSize = new Size(360, 0) // Force wrap within card width
            };
            inner.Controls.Add(_errorLabel, 0, row++);

            // Row 8: Login button (accent colors matching RegisterForm)
            _loginButton = new RoundedButton
            {
                Text = "Sign In",
                Dock = DockStyle.Fill,
                ButtonColorStart = AppTheme.AccentTeal,
                ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186),
                HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 4, 0, 4)
            };
            _loginButton.Click += LoginButton_Click;
            inner.Controls.Add(_loginButton, 0, row++);

            // Row 9: Register link (inside card)
            _registerLink = new LinkLabel
            {
                Text = "Don't have an account? Sign Up",
                Font = AppTheme.BodyRegular,
                LinkColor = AppTheme.AccentCyan,
                ActiveLinkColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            _registerLink.LinkClicked += (s, e) => new RegisterForm().ShowDialog(this);
            inner.Controls.Add(_registerLink, 0, row++);

            // Enter key
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoginButton_Click(this, EventArgs.Empty); };
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

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            _errorLabel.Text = "";
            _emailBox.ClearError();
            _passwordBox.ClearError();

            // Validate
            if (string.IsNullOrWhiteSpace(_emailBox.Text))
            {
                _emailBox.ShowError();
                _errorLabel.Text = "Please enter your email";
                return;
            }
            if (string.IsNullOrWhiteSpace(_passwordBox.Text))
            {
                _passwordBox.ShowError();
                _errorLabel.Text = "Please enter your password";
                return;
            }

            _loginButton.Enabled = false;
            _loginButton.Text = "Signing in...";
            try
            {
                var (success, message, user) = _authService.Login(_emailBox.Text.Trim(), _passwordBox.Text);
                if (success && user != null)
                {
                    var mainForm = new MainForm();
                    mainForm.FormClosed += (s, args) => Close();
                    mainForm.Show();
                    Hide();
                }
                else
                {
                    _errorLabel.Text = message;
                    AnimationHelper.ShakeControl(_errorLabel, 4, 300);
                }
            }
            catch (Exception ex)
            {
                _errorLabel.Text = $"Connection error: {ex.Message}";
            }
            finally
            {
                _loginButton.Enabled = true;
                _loginButton.Text = "Sign In";
            }
        }

    }
}
