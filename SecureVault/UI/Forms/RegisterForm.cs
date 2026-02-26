// ============================================
// SecureVault - Register Form (Layout Refactored)
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

        public RegisterForm()
        {
            Text = "SecureVault – Register";
            Size = new Size(480, 660);
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
            Load += (s, e) => AnimationHelper.FadeIn(this, 400);
        }

        private void BuildUI()
        {
            // Centering table
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

            var card = new Panel
            {
                Size = new Size(420, 540),
                BackColor = Color.Transparent
            };
            card.Paint += (s, e) => AppTheme.PaintGlassCard(e.Graphics,
                new Rectangle(0, 0, card.Width - 1, card.Height - 1), AppTheme.RadiusLarge);
            outer.Controls.Add(card, 1, 1);

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                BackColor = Color.Transparent,
                Padding = new Padding(30, 25, 30, 20)
            };
            // Title
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            // Name label + input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            // Email label + input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            // Password label + input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            // Confirm label + input
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            // Error
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            // Button
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
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
            _passwordBox = MakeInput("Min 6 chars, upper+lower+digit", true); inner.Controls.Add(_passwordBox, 0, row++);

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
                ButtonColorStart = AppTheme.AccentTeal, ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186),
                HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 4, 0, 4)
            };
            _registerButton.Click += RegisterButton_Click;
            inner.Controls.Add(_registerButton, 0, row);

            // Enter key submits the form
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) RegisterButton_Click(this, EventArgs.Empty); };
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
                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else _errorLabel.Text = message;
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
