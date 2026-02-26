// ============================================
// SecureVault - Modern Progress Bar
// Gradient fill, rounded, animated shimmer
// ============================================

using System.Drawing.Drawing2D;
using SecureVault.UI.Theme;

namespace SecureVault.UI.Controls
{
    /// <summary>
    /// A premium progress bar with gradient fill, rounded corners,
    /// and animated shimmer effect during progress.
    /// </summary>
    public class ModernProgressBar : Control
    {
        private int _value;
        private int _maximum = 100;
        private float _shimmerOffset;
        private System.Windows.Forms.Timer? _shimmerTimer;

        public Color BarColorStart { get; set; } = AppTheme.GradientStart;
        public Color BarColorEnd { get; set; } = AppTheme.GradientEnd;
        public Color TrackColor { get; set; } = AppTheme.SurfaceDark;
        public int CornerRadius { get; set; } = 6;
        public bool ShowShimmer { get; set; } = true;

        public int Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(1, value); Invalidate(); }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, 0, _maximum);
                if (_value > 0 && _value < _maximum && ShowShimmer)
                    StartShimmer();
                else
                    StopShimmer();
                Invalidate();
            }
        }

        public ModernProgressBar()
        {
            Size = new Size(300, 8);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        private void StartShimmer()
        {
            if (_shimmerTimer != null) return;
            _shimmerTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _shimmerTimer.Tick += (s, e) =>
            {
                _shimmerOffset += 5;
                if (_shimmerOffset > Width) _shimmerOffset = -40;
                Invalidate();
            };
            _shimmerTimer.Start();
        }

        private void StopShimmer()
        {
            _shimmerTimer?.Stop();
            _shimmerTimer?.Dispose();
            _shimmerTimer = null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var trackRect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Track
            using var trackPath = AppTheme.CreateRoundedRect(trackRect, CornerRadius);
            using var trackBrush = new SolidBrush(TrackColor);
            g.FillPath(trackBrush, trackPath);

            // Fill
            float percent = (float)_value / _maximum;
            int fillWidth = (int)(trackRect.Width * percent);
            if (fillWidth > 0)
            {
                var fillRect = new Rectangle(trackRect.X, trackRect.Y,
                    Math.Max(CornerRadius * 2, fillWidth), trackRect.Height);
                using var fillPath = AppTheme.CreateRoundedRect(fillRect, CornerRadius);
                using var fillBrush = new LinearGradientBrush(fillRect, BarColorStart, BarColorEnd, 0f);
                g.FillPath(fillBrush, fillPath);

                // Shimmer highlight
                if (_shimmerTimer != null)
                {
                    var prevClip = g.Clip;
                    g.SetClip(fillPath, CombineMode.Intersect);
                    int shimmerWidth = 40;
                    var shimmerRect = new Rectangle((int)_shimmerOffset, 0, shimmerWidth, Height);
                    using var shimmerBrush = new LinearGradientBrush(shimmerRect,
                        Color.FromArgb(0, 255, 255, 255),
                        Color.FromArgb(60, 255, 255, 255), 0f);
                    var blend = new ColorBlend(3)
                    {
                        Colors = new[] {
                            Color.FromArgb(0, 255, 255, 255),
                            Color.FromArgb(60, 255, 255, 255),
                            Color.FromArgb(0, 255, 255, 255)
                        },
                        Positions = new[] { 0f, 0.5f, 1f }
                    };
                    shimmerBrush.InterpolationColors = blend;
                    g.FillRectangle(shimmerBrush, shimmerRect);
                    g.Clip = prevClip;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) StopShimmer();
            base.Dispose(disposing);
        }
    }
}
