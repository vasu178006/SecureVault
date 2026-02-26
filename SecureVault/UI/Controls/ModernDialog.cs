// ============================================
// SecureVault - Modern Dialog
// Custom modal with backdrop, scale-in animation,
// themed styling. Replaces MessageBox.
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium-styled modal dialog with backdrop overlay,
    /// scale-in animation, and themed buttons.
    /// Replaces system MessageBox for blocking confirmations.
    /// </summary>
    public class ModernDialog : Form
    {
        private readonly string _title;
        private readonly string _message;
        private readonly string _confirmText;
        private readonly string _cancelText;
        private readonly bool _showCancel;
        private readonly Color _confirmColor;

        private ModernDialog(string title, string message, string confirmText,
            string? cancelText, Color? confirmColor)
        {
            _title = title;
            _message = message;
            _confirmText = confirmText;
            _cancelText = cancelText ?? "Cancel";
            _showCancel = cancelText != null;
            _confirmColor = confirmColor ?? AppTheme.GradientStart;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = AppTheme.PrimaryDark; // Solid Fallback if shadow masking fails
            Size = new Size(420, 220); // Just the size of the pop-up
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BuildUI();
            Load += (s, e) => AnimationHelper.ScaleIn(this, 250);
        }

        private void BuildUI()
        {
            // Dialog card
            var card = new Panel
            {
                Size = new Size(400, 200),
                Location = new Point(10, 10),
                BackColor = Color.Transparent
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                AppTheme.DrawShadow(g, rect, AppTheme.RadiusCard, 10);
                using var path = AppTheme.CreateRoundedRect(rect, AppTheme.RadiusCard);
                using var bgBrush = new SolidBrush(AppTheme.SurfaceDark);
                g.FillPath(bgBrush, path);
                using var borderPen = new Pen(Color.FromArgb(30, 255, 255, 255), 1f);
                g.DrawPath(borderPen, path);
            };

            // Title
            var titleLabel = new Label
            {
                Text = _title,
                Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(28, 24),
                AutoSize = true
            };
            card.Controls.Add(titleLabel);

            // Message
            var msgLabel = new Label
            {
                Text = _message,
                Font = AppTheme.BodyRegular,
                ForeColor = AppTheme.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(28, 60),
                Size = new Size(344, 60),
                MaximumSize = new Size(344, 60)
            };
            card.Controls.Add(msgLabel);

            // Buttons
            var btnPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Location = new Point(28, 140),
                Size = new Size(344, 44),
                BackColor = Color.Transparent
            };

            var confirmBtn = new RoundedButton
            {
                Text = _confirmText,
                Size = new Size(120, 40),
                ButtonColorStart = _confirmColor,
                ButtonColorEnd = AppTheme.LerpColor(_confirmColor, AppTheme.GradientEnd, 0.5f),
                HoverColorStart = AppTheme.LerpColor(_confirmColor, Color.White, 0.15f),
                HoverColorEnd = AppTheme.LerpColor(AppTheme.GradientEnd, Color.White, 0.15f),
                CornerRadius = 10,
                Margin = new Padding(0, 0, 8, 0)
            };
            confirmBtn.Click += (s, e) => { DialogResult = DialogResult.Yes; Close(); };
            btnPanel.Controls.Add(confirmBtn);

            if (_showCancel)
            {
                var cancelBtn = new RoundedButton
                {
                    Text = _cancelText,
                    Size = new Size(100, 40),
                    IsGradient = false,
                    FlatColor = AppTheme.SurfaceMid,
                    CornerRadius = 10,
                    Margin = new Padding(0, 0, 8, 0)
                };
                cancelBtn.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
                btnPanel.Controls.Add(cancelBtn);
            }

            card.Controls.Add(btnPanel);
            Controls.Add(card);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw a subtle border around the form itself to tie into the card
            e.Graphics.Clear(AppTheme.SurfaceDark);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // Removed WS_EX_TRANSPARENT as it breaks WinForms hit testing and child control painting
                return cp;
            }
        }

        // ═══════════════════════════════════════════
        //  Static Factory Methods
        // ═══════════════════════════════════════════

        private static Form? ShowBackdrop(IWin32Window? owner)
        {
            if (owner is Form parentForm)
            {
                var shadow = new Form
                {
                    StartPosition = FormStartPosition.Manual,
                    FormBorderStyle = FormBorderStyle.None,
                    Opacity = 0.50,
                    BackColor = Color.Black,
                    ShowInTaskbar = false,
                    Location = parentForm.PointToScreen(Point.Empty),
                    Size = parentForm.ClientSize
                };
                shadow.Show(parentForm);
                return shadow;
            }
            return null;
        }

        /// <summary>
        /// Shows a confirmation dialog with Yes/Cancel buttons.
        /// Returns true if user confirmed.
        /// </summary>
        public static bool Confirm(string title, string message,
            string confirmText = "Confirm", string cancelText = "Cancel",
            IWin32Window? owner = null)
        {
            using var shadow = ShowBackdrop(owner);
            using var dlg = new ModernDialog(title, message, confirmText, cancelText, null);
            bool result = dlg.ShowDialog(shadow ?? owner) == DialogResult.Yes;
            shadow?.Close();
            return result;
        }

        /// <summary>
        /// Shows a destructive confirmation dialog (red accent).
        /// </summary>
        public static bool ConfirmDelete(string title, string message,
            string confirmText = "Delete", IWin32Window? owner = null)
        {
            using var shadow = ShowBackdrop(owner);
            using var dlg = new ModernDialog(title, message, confirmText, "Cancel", AppTheme.Error);
            bool result = dlg.ShowDialog(shadow ?? owner) == DialogResult.Yes;
            shadow?.Close();
            return result;
        }

        /// <summary>
        /// Shows an info dialog with only an OK button.
        /// </summary>
        public static void ShowInfo(string title, string message, IWin32Window? owner = null)
        {
            using var shadow = ShowBackdrop(owner);
            using var dlg = new ModernDialog(title, message, "OK", null, AppTheme.Info);
            dlg.ShowDialog(shadow ?? owner);
            shadow?.Close();
        }
    }
}
