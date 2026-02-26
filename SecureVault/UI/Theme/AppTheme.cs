// ============================================
// SecureVault - Application Theme
// Centralized colors, fonts, and gradient definitions
// ============================================

using System.Drawing.Drawing2D;

namespace SecureVault.UI.Theme
{
    /// <summary>
    /// Centralized theme constants for the SecureVault application.
    /// Deep Purple → Electric Blue gradient with dark mode.
    /// </summary>
    public static class AppTheme
    {
        // ── Primary Colors ──
        public static readonly Color PrimaryDark = Color.FromArgb(26, 26, 46);       // #1A1A2E
        public static readonly Color PrimaryMid = Color.FromArgb(22, 33, 62);        // #16213E
        public static readonly Color PrimaryLight = Color.FromArgb(30, 39, 73);      // #1E2749

        // ── Gradient Colors ──
        public static readonly Color GradientStart = Color.FromArgb(108, 59, 170);   // #6C3BAA - Deep Purple
        public static readonly Color GradientEnd = Color.FromArgb(37, 99, 235);      // #2563EB - Electric Blue
        public static readonly Color GradientMid = Color.FromArgb(72, 79, 203);      // middle blend

        // ── Accent Colors ──
        public static readonly Color AccentTeal = Color.FromArgb(20, 184, 166);      // #14B8A6
        public static readonly Color AccentCyan = Color.FromArgb(6, 182, 212);       // #06B6D4
        public static readonly Color AccentGlow = Color.FromArgb(147, 130, 255);     // Soft neon purple
        public static readonly Color AccentPink = Color.FromArgb(236, 72, 153);      // #EC4899

        // ── Surface Colors ──
        public static readonly Color SurfaceDark = Color.FromArgb(30, 30, 50);       // Card background
        public static readonly Color SurfaceMid = Color.FromArgb(40, 40, 65);        // Elevated card
        public static readonly Color SurfaceLight = Color.FromArgb(50, 50, 75);      // Hover state
        public static readonly Color SurfaceBorder = Color.FromArgb(60, 60, 90);     // Subtle borders

        // ── Text Colors ──
        public static readonly Color TextPrimary = Color.FromArgb(240, 240, 255);    // Main text
        public static readonly Color TextSecondary = Color.FromArgb(160, 160, 190);  // Muted text
        public static readonly Color TextMuted = Color.FromArgb(120, 120, 150);      // Hint text
        public static readonly Color TextOnAccent = Color.White;

        // ── Status Colors ──
        public static readonly Color Success = Color.FromArgb(34, 197, 94);          // Green
        public static readonly Color Warning = Color.FromArgb(250, 204, 21);         // Yellow
        public static readonly Color Error = Color.FromArgb(239, 68, 68);            // Red
        public static readonly Color Info = Color.FromArgb(59, 130, 246);            // Blue

        // ── Sidebar Colors ──
        public static readonly Color SidebarBg = Color.FromArgb(18, 18, 35);         // Dark sidebar
        public static readonly Color SidebarHover = Color.FromArgb(35, 35, 60);      // Hover highlight
        public static readonly Color SidebarActive = Color.FromArgb(80, 108, 59, 170);  // Active item (semi-transparent purple)

        // ── Font ──
        public static readonly string FontFamily = "Segoe UI";
        public static readonly Font HeadingLarge = new(FontFamily, 24, FontStyle.Bold);
        public static readonly Font HeadingMedium = new(FontFamily, 18, FontStyle.Bold);
        public static readonly Font HeadingSmall = new(FontFamily, 14, FontStyle.Bold);
        public static readonly Font BodyLarge = new(FontFamily, 12, FontStyle.Regular);
        public static readonly Font BodyRegular = new(FontFamily, 10, FontStyle.Regular);
        public static readonly Font BodySmall = new(FontFamily, 9, FontStyle.Regular);
        public static readonly Font ButtonFont = new(FontFamily, 11, FontStyle.Bold);
        public static readonly Font SidebarFont = new(FontFamily, 11, FontStyle.Regular);

        // ── Corner Radius ──
        public const int RadiusSmall = 6;
        public const int RadiusMedium = 10;
        public const int RadiusLarge = 16;
        public const int RadiusXLarge = 24;

        /// <summary>
        /// Creates a linear gradient brush between two points.
        /// </summary>
        public static LinearGradientBrush CreateGradient(Rectangle rect, Color start, Color end, float angle = 135f)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                rect = new Rectangle(0, 0, 1, 1);
            return new LinearGradientBrush(rect, start, end, angle);
        }

        /// <summary>
        /// Creates the primary purple-to-blue gradient.
        /// </summary>
        public static LinearGradientBrush CreatePrimaryGradient(Rectangle rect, float angle = 135f)
        {
            return CreateGradient(rect, GradientStart, GradientEnd, angle);
        }

        /// <summary>
        /// Creates a rounded rectangle GraphicsPath.
        /// </summary>
        public static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            if (d > bounds.Width) d = bounds.Width;
            if (d > bounds.Height) d = bounds.Height;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// Applies shadow effect visually by drawing offset versions.
        /// </summary>
        public static void DrawShadow(Graphics g, Rectangle rect, int radius, int shadowDepth = 5)
        {
            for (int i = shadowDepth; i > 0; i--)
            {
                int alpha = 8 * (shadowDepth - i + 1);
                using var shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                var shadowRect = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);
                using var shadowPath = CreateRoundedRect(shadowRect, radius);
                g.FillPath(shadowBrush, shadowPath);
            }
        }

        /// <summary>
        /// Paints a glass-morphism style card background.
        /// </summary>
        public static void PaintGlassCard(Graphics g, Rectangle bounds, int radius = RadiusMedium, int shadowDepth = 6)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Shadow
            DrawShadow(g, bounds, radius, shadowDepth);

            // Fill with semi-transparent surface
            using var fillBrush = new SolidBrush(Color.FromArgb(200, SurfaceDark.R, SurfaceDark.G, SurfaceDark.B));
            using var path = CreateRoundedRect(bounds, radius);
            g.FillPath(fillBrush, path);

            // Subtle border
            using var borderPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f);
            g.DrawPath(borderPen, path);
        }
    }
}
