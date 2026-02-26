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
        private float _bgAnimPhase;
        private System.Windows.Forms.Timer? _bgTimer;

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
            Load += (s, e) =>
            {
                AnimationHelper.FadeIn(this, 500);
                StartBackgroundAnimation();
            };
        }

        private void StartBackgroundAnimation()
        {
            _bgTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _bgTimer.Tick += (s, e) =>
            {
                _bgAnimPhase += 0.006f;
                if (_bgAnimPhase > 2 * Math.PI) _bgAnimPhase -= (float)(2 * Math.PI);
                Invalidate(false);
            };
            _bgTimer.Start();
        }

        private void PaintBackground(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Base gradient
            using var baseBrush = AppTheme.CreatePrimaryGradient(ClientRectangle, 135f);
            g.FillRectangle(baseBrush, ClientRectangle);

            // Animated ambient orbs
            float orb1X = (float)(Math.Sin(_bgAnimPhase) * 80 - 60);
            float orb1Y = (float)(Math.Cos(_bgAnimPhase * 0.7) * 60 - 60);
            using var c1 = new SolidBrush(Color.FromArgb(12, 167, 139, 250));
            g.FillEllipse(c1, orb1X, orb1Y, 450, 450);

            float orb2X = Width - 250 + (float)(Math.Cos(_bgAnimPhase * 0.5) * 50);
            float orb2Y = Height - 250 + (float)(Math.Sin(_bgAnimPhase * 0.8) * 40);
            using var c2 = new SolidBrush(Color.FromArgb(10, 59, 130, 246));
            g.FillEllipse(c2, orb2X, orb2Y, 400, 400);

            float orb3X = Width / 2 + (float)(Math.Sin(_bgAnimPhase * 1.2) * 100);
            float orb3Y = Height / 3 + (float)(Math.Cos(_bgAnimPhase * 0.6) * 70);
            using var c3 = new SolidBrush(Color.FromArgb(8, 244, 114, 182));
            g.FillEllipse(c3, orb3X, orb3Y, 300, 300);
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
                Size = new Size(440, 510),
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusLarge, 10);
            outer.Controls.Add(card, 1, 1);

            // Inner layout inside card
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = Color.Transparent,
                Padding = new Padding(32, 30, 32, 24)
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Shield icon
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Title
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // Subtitle
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Email label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // Email box
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));   // Pass label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // Pass box
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));   // Error label
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));   // Login button
            card.Controls.Add(inner);

            // Row 0: Shield icon
            inner.Controls.Add(new Label
            {
                Text = "🛡️",
                Font = new Font("Segoe UI Emoji", 24),
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            }, 0, 0);

            // Row 1: Title
            inner.Controls.Add(new Label
            {
                Text = "SecureVault",
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            }, 0, 1);

            // Row 2: Subtitle
            inner.Controls.Add(new Label
            {
                Text = "Sign in to your digital locker",
                Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.TextSecondary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter
            }, 0, 2);

            // Row 3: Email label
            inner.Controls.Add(MakeFieldLabel("Email Address"), 0, 3);

            // Row 4: Email input
            _emailBox = new RoundedTextBox
            {
                PlaceholderText = "Enter your email",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 4)
            };
            inner.Controls.Add(_emailBox, 0, 4);

            // Row 5: Password label
            inner.Controls.Add(MakeFieldLabel("Password"), 0, 5);

            // Row 6: Password input
            _passwordBox = new RoundedTextBox
            {
                PlaceholderText = "Enter your password",
                IsPassword = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 4)
            };
            inner.Controls.Add(_passwordBox, 0, 6);

            // Row 7: Error label
            _errorLabel = new Label
            {
                Text = "",
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.Error,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            inner.Controls.Add(_errorLabel, 0, 7);

            // Row 8: Login button
            _loginButton = new RoundedButton
            {
                Text = "Sign In",
                Dock = DockStyle.Fill,
                CornerRadius = AppTheme.RadiusMedium,
                Margin = new Padding(0, 4, 0, 0)
            };
            _loginButton.Click += LoginButton_Click;
            inner.Controls.Add(_loginButton, 0, 8);

            // Register link (below card)
            _registerLink = new LinkLabel
            {
                Text = "Don't have an account? Sign Up",
                Font = AppTheme.BodyRegular,
                LinkColor = AppTheme.AccentCyan,
                ActiveLinkColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0),
                Anchor = AnchorStyles.Top
            };
            _registerLink.LinkClicked += (s, e) => new RegisterForm().ShowDialog(this);
            outer.Controls.Add(_registerLink, 1, 2);

            // Enter key
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoginButton_Click(this, EventArgs.Empty); };
        }

        private Label MakeFieldLabel(string text) => new()
        {
            Text = text,
            Font = AppTheme.BodySmall,
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(2, 0, 0, 0)
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
                    _bgTimer?.Stop();
                    _bgTimer?.Dispose();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bgTimer?.Stop();
                _bgTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
