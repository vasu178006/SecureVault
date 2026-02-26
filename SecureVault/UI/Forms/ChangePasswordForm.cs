// ============================================
// SecureVault - Change Password Form (Layout Refactored)
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Forms
{
    public class ChangePasswordForm : Form
    {
        private readonly AuthService _authService = new();
        private RoundedTextBox _currentPassBox = null!;
        private RoundedTextBox _newPassBox = null!;
        private RoundedTextBox _confirmPassBox = null!;
        private RoundedButton _changeButton = null!;
        private Label _errorLabel = null!;

        public ChangePasswordForm()
        {
            Text = "Change Password";
            Size = new Size(420, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;

            BuildUI();
            Load += (s, e) => AnimationHelper.FadeIn(this, 300);

            // Enter key submits
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) ChangeButton_Click(this, EventArgs.Empty); };
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 9,
                BackColor = Color.Transparent,
                Padding = new Padding(30, 20, 30, 20)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // Title
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Label
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Input
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Label
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Input
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Label
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Input
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // Error
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));  // Button
            Controls.Add(layout);

            int r = 0;
            layout.Controls.Add(new Label
            {
                Text = "🔑 Change Password", Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter
            }, 0, r++);

            layout.Controls.Add(MakeLabel("Current Password"), 0, r++);
            _currentPassBox = MakeInput("Enter current password", true); layout.Controls.Add(_currentPassBox, 0, r++);

            layout.Controls.Add(MakeLabel("New Password"), 0, r++);
            _newPassBox = MakeInput("Enter new password", true); layout.Controls.Add(_newPassBox, 0, r++);

            layout.Controls.Add(MakeLabel("Confirm New Password"), 0, r++);
            _confirmPassBox = MakeInput("Re-enter new password", true); layout.Controls.Add(_confirmPassBox, 0, r++);

            _errorLabel = new Label
            {
                Text = "", Font = AppTheme.BodySmall, ForeColor = AppTheme.Error,
                BackColor = Color.Transparent, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            layout.Controls.Add(_errorLabel, 0, r++);

            _changeButton = new RoundedButton
            {
                Text = "Update Password", Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 4)
            };
            _changeButton.Click += ChangeButton_Click;
            layout.Controls.Add(_changeButton, 0, r);
        }

        private Label MakeLabel(string text) => new()
        {
            Text = text, Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };

        private RoundedTextBox MakeInput(string placeholder, bool isPassword) => new()
        {
            PlaceholderText = placeholder, IsPassword = isPassword,
            Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 4)
        };

        private void ChangeButton_Click(object? sender, EventArgs e)
        {
            _errorLabel.Text = "";
            var (success, message) = _authService.ChangePassword(
                SessionManager.CurrentUserID,
                _currentPassBox.Text, _newPassBox.Text, _confirmPassBox.Text);
            if (success) { MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); Close(); }
            else _errorLabel.Text = message;
        }
    }
}
