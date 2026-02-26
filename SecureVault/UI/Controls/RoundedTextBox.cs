// ============================================
// SecureVault - Rounded TextBox
// Modern styled text input with dark theme
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A modern styled text box with rounded border and dark theme.
    /// Uses a Panel wrapper for the rounded border effect.
    /// </summary>
    public class RoundedTextBox : UserControl
    {
        private readonly TextBox _textBox;
        private readonly Label _placeholderLabel;
        private bool _isFocused;
        public string PlaceholderText { get; set; } = "";

        public Color BorderColor { get; set; } = AppTheme.SurfaceBorder;
        public Color FocusBorderColor { get; set; } = AppTheme.AccentCyan;
        public int CornerRadius { get; set; } = AppTheme.RadiusSmall;
        public bool IsPassword
        {
            get => _textBox.UseSystemPasswordChar;
            set => _textBox.UseSystemPasswordChar = value;
        }

        public override string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        public bool ReadOnly
        {
            get => _textBox.ReadOnly;
            set => _textBox.ReadOnly = value;
        }

        public int MaxLength
        {
            get => _textBox.MaxLength;
            set => _textBox.MaxLength = value;
        }

        public new event EventHandler? TextChanged;

        public RoundedTextBox()
        {
            Size = new Size(300, 44);
            BackColor = AppTheme.SurfaceDark;
            Padding = new Padding(14, 0, 14, 0);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = AppTheme.SurfaceDark,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyLarge,
                Dock = DockStyle.Fill
            };

            _placeholderLabel = new Label
            {
                ForeColor = AppTheme.TextMuted,
                Font = AppTheme.BodyLarge,
                BackColor = Color.Transparent,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.IBeam
            };

            _textBox.GotFocus += (s, e) =>
            {
                _isFocused = true;
                UpdatePlaceholder();
                Invalidate();
            };

            _textBox.LostFocus += (s, e) =>
            {
                _isFocused = false;
                UpdatePlaceholder();
                Invalidate();
            };

            _textBox.TextChanged += (s, e) =>
            {
                UpdatePlaceholder();
                TextChanged?.Invoke(this, e);
            };

            _placeholderLabel.Click += (s, e) => _textBox.Focus();

            Controls.Add(_textBox);
            Controls.Add(_placeholderLabel);
            _textBox.BringToFront();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _placeholderLabel.Text = PlaceholderText;
            UpdatePlaceholder();
        }

        private void UpdatePlaceholder()
        {
            _placeholderLabel.Visible = string.IsNullOrEmpty(_textBox.Text) && !_isFocused;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? AppTheme.PrimaryDark);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);

            // Background
            using var bgBrush = new SolidBrush(AppTheme.SurfaceDark);
            g.FillPath(bgBrush, path);

            // Border
            var borderColor = _isFocused ? FocusBorderColor : BorderColor;
            float borderWidth = _isFocused ? 2f : 1f;
            using var borderPen = new Pen(borderColor, borderWidth);
            g.DrawPath(borderPen, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_textBox == null) return;
            _textBox.Location = new Point(Padding.Left, (Height - _textBox.Height) / 2);
            _textBox.Width = Width - Padding.Left - Padding.Right;
        }

        /// <summary>
        /// Overrides Focus to focus the inner TextBox.
        /// </summary>
        public new bool Focus()
        {
            return _textBox.Focus();
        }

        /// <summary>
        /// Selects all text in the TextBox.
        /// </summary>
        public void SelectAll()
        {
            _textBox.SelectAll();
        }
    }
}
