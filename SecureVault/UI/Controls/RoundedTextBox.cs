// ============================================
// SecureVault - Rounded TextBox (Redesigned)
// Floating label, animated focus border,
// validation states, and icon support
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium text input with floating label animation,
    /// smooth focus border transition, and validation states.
    /// </summary>
    public class RoundedTextBox : UserControl
    {
        private readonly TextBox _textBox;
        private readonly Label _placeholderLabel;
        private bool _isFocused;
        private float _focusProgress;     // 0 = unfocused, 1 = focused
        private System.Windows.Forms.Timer? _focusTimer;

        public string PlaceholderText { get; set; } = "";
        public Color BorderColor { get; set; } = AppTheme.SurfaceBorder;
        public Color FocusBorderColor { get; set; } = AppTheme.AccentCyan;
        public Color ErrorBorderColor { get; set; } = AppTheme.Error;
        public int CornerRadius { get; set; } = AppTheme.RadiusSmall;

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; Invalidate(); }
        }

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
            Size = new Size(300, 48);
            BackColor = AppTheme.SurfaceDark;
            Padding = new Padding(14, 0, 14, 0);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = AppTheme.SurfaceDark,
                ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyLarge,
                Anchor = AnchorStyles.None
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
                StartFocusAnimation(1f);
            };

            _textBox.LostFocus += (s, e) =>
            {
                _isFocused = false;
                UpdatePlaceholder();
                StartFocusAnimation(0f);
            };

            _textBox.TextChanged += (s, e) =>
            {
                UpdatePlaceholder();
                if (_hasError) { _hasError = false; Invalidate(); }
                TextChanged?.Invoke(this, e);
            };

            _placeholderLabel.Click += (s, e) => _textBox.Focus();
            Click += (s, e) => _textBox.Focus();

            Controls.Add(_textBox);
            Controls.Add(_placeholderLabel);
            _placeholderLabel.BringToFront();
        }

        private void StartFocusAnimation(float target)
        {
            _focusTimer?.Stop();
            _focusTimer?.Dispose();
            _focusTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float step = target > _focusProgress ? 0.15f : -0.15f;
            _focusTimer.Tick += (s, e) =>
            {
                _focusProgress += step;
                if ((step > 0 && _focusProgress >= target) ||
                    (step < 0 && _focusProgress <= target))
                {
                    _focusProgress = target;
                    _focusTimer.Stop();
                    _focusTimer.Dispose();
                    _focusTimer = null;
                }
                Invalidate();
            };
            _focusTimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _placeholderLabel.Text = PlaceholderText;
            PositionTextBox();
            UpdatePlaceholder();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _placeholderLabel.Text = PlaceholderText;
            PositionTextBox();
            UpdatePlaceholder();
        }

        private void UpdatePlaceholder()
        {
            _placeholderLabel.Visible = string.IsNullOrEmpty(_textBox.Text) && !_isFocused;
        }

        private void PositionTextBox()
        {
            if (_textBox == null) return;
            _textBox.Width = Width - Padding.Left - Padding.Right;
            _textBox.Location = new Point(Padding.Left, (Height - _textBox.Height) / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(AppTheme.GetEffectiveBackColor(Parent));

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = AppTheme.CreateRoundedRect(rect, CornerRadius);

            // Background
            using var bgBrush = new SolidBrush(AppTheme.SurfaceDark);
            g.FillPath(bgBrush, path);

            // Border with animated transition
            Color currentBorder;
            float borderWidth;
            if (_hasError)
            {
                currentBorder = ErrorBorderColor;
                borderWidth = 2f;
            }
            else
            {
                currentBorder = AppTheme.LerpColor(BorderColor, FocusBorderColor, _focusProgress);
                borderWidth = 1f + _focusProgress * 0.8f;
            }
            using var borderPen = new Pen(currentBorder, borderWidth);
            g.DrawPath(borderPen, path);

            // Glow on focus
            if (_focusProgress > 0.01f && !_hasError)
            {
                AppTheme.PaintGlowBorder(g, rect, CornerRadius, FocusBorderColor, _focusProgress * 0.3f);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionTextBox();
        }

        public new bool Focus() => _textBox.Focus();
        public void SelectAll() => _textBox.SelectAll();

        /// <summary>Triggers a shake animation for validation error feedback.</summary>
        public void ShowError()
        {
            HasError = true;
            AnimationHelper.ShakeControl(this, 6, 400);
        }

        /// <summary>Clears the error state.</summary>
        public void ClearError() => HasError = false;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _focusTimer?.Stop();
                _focusTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
