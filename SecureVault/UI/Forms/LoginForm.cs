// ============================================
// SecureVault - Login Form (Layout Refactored)
// TableLayoutPanel-based centered card
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
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font(AppTheme.FontFamily, 10);

            Paint += PaintBackground;
            BuildUI();
            Load += (s, e) => AnimationHelper.FadeIn(this, 500);
        }

        private void PaintBackground(object? sender, PaintEventArgs e)
        {
            using var brush = AppTheme.CreatePrimaryGradient(ClientRectangle, 135f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var c1 = new SolidBrush(Color.FromArgb(15, 255, 255, 255));
            e.Graphics.FillEllipse(c1, -100, -100, 400, 400);
            using var c2 = new SolidBrush(Color.FromArgb(10, 255, 255, 255));
            e.Graphics.FillEllipse(c2, Width - 200, Height - 200, 350, 350);
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

            // Glass card panel
            var card = new Panel
            {
                Size = new Size(420, 500),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusLarge, 8);
            outer.Controls.Add(card, 1, 1);

            // Inner layout inside card
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = Color.Transparent,
                Padding = new Padding(35, 30, 35, 25)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Title
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Subtitle
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Email label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Email box
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Pass label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));  // Pass box
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // Error label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));  // Login button
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Register link
            card.Controls.Add(inner);

            // Row 0: Title
            inner.Controls.Add(new Label
            {
                Text = "🔒 SecureVault",
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 0);

            // Row 1: Subtitle
            inner.Controls.Add(new Label
            {
                Text = "Sign in to your digital locker",
                Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.TextSecondary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            }, 0, 1);

            // Row 2: Email label
            inner.Controls.Add(MakeFieldLabel("Email Address"), 0, 2);

            // Row 3: Email input
            _emailBox = new RoundedTextBox
            {
                PlaceholderText = "Enter your email",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 4)
            };
            inner.Controls.Add(_emailBox, 0, 3);

            // Row 4: Password label
            inner.Controls.Add(MakeFieldLabel("Password"), 0, 4);

            // Row 5: Password input
            _passwordBox = new RoundedTextBox
            {
                PlaceholderText = "Enter your password",
                IsPassword = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 4)
            };
            inner.Controls.Add(_passwordBox, 0, 5);

            // Row 6: Error label
            _errorLabel = new Label
            {
                Text = "",
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.Error,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            inner.Controls.Add(_errorLabel, 0, 6);

            // Row 7: Login button
            _loginButton = new RoundedButton
            {
                Text = "Sign In",
                Dock = DockStyle.Fill,
                CornerRadius = AppTheme.RadiusMedium,
                Margin = new Padding(0, 4, 0, 4)
            };
            _loginButton.Click += LoginButton_Click;
            inner.Controls.Add(_loginButton, 0, 7);

            // Row 8: Register link
            _registerLink = new LinkLabel
            {
                Text = "Don't have an account? Sign Up",
                Font = AppTheme.BodyRegular,
                LinkColor = AppTheme.AccentCyan,
                ActiveLinkColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _registerLink.LinkClicked += (s, e) => new RegisterForm().ShowDialog(this);
            inner.Controls.Add(_registerLink, 0, 8);

            // Enter key
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoginButton_Click(this, EventArgs.Empty); };
        }

        private Label MakeFieldLabel(string text) => new()
        {
            Text = text,
            Font = AppTheme.BodyRegular,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(0)
        };

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            _errorLabel.Text = "";
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
