// ============================================
// SecureVault - Application Theme (Redesigned)
// Premium 2026 dark-mode palette, refined fonts,
// glass-card helpers, and glow utilities
// ============================================

using System.Drawing.Drawing2D;

namespace SecureVault.UI.Theme
{
    /// <summary>
    /// Centralized theme constants for the SecureVault application.
    /// Deep Violet → Electric Blue gradient with refined dark surfaces.
    /// </summary>
    public static class AppTheme
    {
        // ── Primary Backgrounds ──
        public static readonly Color PrimaryDark   = Color.FromArgb(15, 15, 26);       // #0F0F1A – main bg
        public static readonly Color PrimaryMid    = Color.FromArgb(20, 20, 40);       // #141428
        public static readonly Color PrimaryLight  = Color.FromArgb(26, 26, 53);       // #1A1A35

        // ── Gradient Accent ──
        public static readonly Color GradientStart = Color.FromArgb(124, 58, 237);     // #7C3AED – vibrant violet
        public static readonly Color GradientEnd   = Color.FromArgb(59, 130, 246);     // #3B82F6 – electric blue
        public static readonly Color GradientMid   = Color.FromArgb(91, 94, 241);      // blend

        // ── Accent Colors ──
        public static readonly Color AccentTeal    = Color.FromArgb(20, 184, 166);     // #14B8A6
        public static readonly Color AccentCyan    = Color.FromArgb(34, 211, 238);     // #22D3EE – brighter
        public static readonly Color AccentGlow    = Color.FromArgb(167, 139, 250);    // #A78BFA – pastel purple
        public static readonly Color AccentPink    = Color.FromArgb(244, 114, 182);    // #F472B6 – softer pink

        // ── Surface Colors ──
        public static readonly Color SurfaceDark   = Color.FromArgb(22, 22, 43);       // #16162B – card bg
        public static readonly Color SurfaceMid    = Color.FromArgb(30, 30, 56);       // #1E1E38 – elevated
        public static readonly Color SurfaceLight  = Color.FromArgb(42, 42, 72);       // #2A2A48 – hover
        public static readonly Color SurfaceBorder = Color.FromArgb(46, 46, 80);       // #2E2E50 – subtle

        // ── Text Colors ──
        public static readonly Color TextPrimary   = Color.FromArgb(241, 245, 249);    // #F1F5F9
        public static readonly Color TextSecondary = Color.FromArgb(148, 163, 184);    // #94A3B8
        public static readonly Color TextMuted     = Color.FromArgb(100, 116, 139);    // #64748B
        public static readonly Color TextOnAccent  = Color.White;

        // ── Status Colors ──
        public static readonly Color Success = Color.FromArgb(34, 197, 94);            // #22C55E
        public static readonly Color Warning = Color.FromArgb(250, 204, 21);           // #FACC15
        public static readonly Color Error   = Color.FromArgb(239, 68, 68);            // #EF4444
        public static readonly Color Info    = Color.FromArgb(59, 130, 246);           // #3B82F6

        // ── Sidebar Colors ──
        public static readonly Color SidebarBg     = Color.FromArgb(11, 11, 24);       // #0B0B18
        public static readonly Color SidebarHover  = Color.FromArgb(30, 30, 55);       // subtle hover
        public static readonly Color SidebarActive = Color.FromArgb(60, 124, 58, 237); // semi-transparent violet

        // ── Font ──
        private static readonly string[] _fontFamilies = { "Segoe UI Variable", "Segoe UI" };
        public static readonly string FontFamily = PickAvailableFont(_fontFamilies);
        public static readonly string MonoFontFamily = PickAvailableFont(new[] { "Cascadia Code", "Consolas" });

        public static readonly Font HeadingLarge  = new(FontFamily, 26, FontStyle.Bold);
        public static readonly Font HeadingMedium = new(FontFamily, 20, FontStyle.Bold);
        public static readonly Font HeadingSmall  = new(FontFamily, 15, FontStyle.Bold);
        public static readonly Font BodyLarge     = new(FontFamily, 13, FontStyle.Regular);
        public static readonly Font BodyRegular   = new(FontFamily, 11, FontStyle.Regular);
        public static readonly Font BodySmall     = new(FontFamily, 9.5f, FontStyle.Regular);
        public static readonly Font ButtonFont    = new(FontFamily, 11, FontStyle.Bold);
        public static readonly Font SidebarFont   = new(FontFamily, 11, FontStyle.Regular);
        public static readonly Font MonoFont      = new(MonoFontFamily, 10, FontStyle.Regular);

        // ── Corner Radius ──
        public const int RadiusSmall  = 8;
        public const int RadiusMedium = 12;
        public const int RadiusLarge  = 16;
        public const int RadiusXLarge = 24;
        public const int RadiusCard   = 14;

        // ── Spacing ──
        public const int SpacingXS = 4;
        public const int SpacingSM = 8;
        public const int SpacingMD = 16;
        public const int SpacingLG = 24;
        public const int SpacingXL = 32;

        // ═══════════════════════════════════════════
        //  Drawing Helpers
        // ═══════════════════════════════════════════

        /// <summary>
        /// Picks the first installed font family from the list.
        /// </summary>
        private static string PickAvailableFont(string[] candidates)
        {
            var installed = new HashSet<string>(
                System.Drawing.FontFamily.Families.Select(f => f.Name),
                StringComparer.OrdinalIgnoreCase);
            foreach (var name in candidates)
                if (installed.Contains(name)) return name;
            return "Segoe UI";
        }

        /// <summary>
        /// Creates a linear gradient brush between two colors.
        /// </summary>
        public static LinearGradientBrush CreateGradient(Rectangle rect, Color start, Color end, float angle = 135f)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
                rect = new Rectangle(0, 0, 1, 1);
            return new LinearGradientBrush(rect, start, end, angle);
        }

        /// <summary>
        /// Creates the primary violet-to-blue gradient.
        /// </summary>
        public static LinearGradientBrush CreatePrimaryGradient(Rectangle rect, float angle = 135f)
            => CreateGradient(rect, GradientStart, GradientEnd, angle);

        /// <summary>
        /// Creates a rounded-rectangle GraphicsPath.
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
        /// Draws a soft multi-layer shadow beneath a rounded rectangle.
        /// </summary>
        public static void DrawShadow(Graphics g, Rectangle rect, int radius, int shadowDepth = 6)
        {
            for (int i = shadowDepth; i > 0; i--)
            {
                int alpha = 6 * (shadowDepth - i + 1);
                using var shadowBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                var shadowRect = new Rectangle(rect.X + i, rect.Y + i, rect.Width, rect.Height);
                using var shadowPath = CreateRoundedRect(shadowRect, radius);
                g.FillPath(shadowBrush, shadowPath);
            }
        }

        /// <summary>
        /// Resolves the effective (non-transparent) background color
        /// by walking up the parent control chain.
        /// </summary>
        public static Color GetEffectiveBackColor(Control? control)
        {
            while (control != null)
            {
                if (control.BackColor != Color.Transparent &&
                    control.BackColor != Color.Empty &&
                    control.BackColor.A > 0)
                    return control.BackColor;
                control = control.Parent;
            }
            return PrimaryDark;
        }

        /// <summary>
        /// Paints a premium glass-morphism card with shadow, fill, inner highlight, and border.
        /// </summary>
        public static void PaintGlassCard(Graphics g, Rectangle bounds, int radius = RadiusCard, int shadowDepth = 8)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Shadow
            DrawShadow(g, bounds, radius, shadowDepth);

            // Fill with semi-transparent surface
            using var fillBrush = new SolidBrush(Color.FromArgb(210, SurfaceDark.R, SurfaceDark.G, SurfaceDark.B));
            using var path = CreateRoundedRect(bounds, radius);
            g.FillPath(fillBrush, path);

            // Inner highlight at top (glass reflection)
            var highlightRect = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height / 3);
            using var highlightBrush = new LinearGradientBrush(
                highlightRect,
                Color.FromArgb(8, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                90f);
            using var clipPath = CreateRoundedRect(bounds, radius);
            var prevClip = g.Clip;
            g.SetClip(clipPath, System.Drawing.Drawing2D.CombineMode.Intersect);
            g.FillRectangle(highlightBrush, highlightRect);
            g.Clip = prevClip;

            // Subtle border
            using var borderPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f);
            g.DrawPath(borderPen, path);
        }

        /// <summary>
        /// Paints a glow effect around a rectangle (for hover states).
        /// </summary>
        public static void PaintGlowBorder(Graphics g, Rectangle rect, int radius, Color glowColor, float intensity = 0.5f)
        {
            for (int i = 3; i > 0; i--)
            {
                int alpha = (int)(intensity * 35 * (4 - i));
                var glowRect = new Rectangle(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2);
                using var glowPen = new Pen(Color.FromArgb(alpha, glowColor.R, glowColor.G, glowColor.B), 1f);
                using var glowPath = CreateRoundedRect(glowRect, radius + i);
                g.DrawPath(glowPen, glowPath);
            }
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        public static Color LerpColor(Color from, Color to, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(from.A + (to.A - from.A) * t),
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        /// <summary>
        /// Paints a stat card with shadow, fill, border, and gradient accent stripe.
        /// </summary>
        public static void PaintStatCard(Graphics g, Rectangle rect, int radius,
            Color gradStart, Color gradEnd, float hoverProgress, int shadowDepth = 6)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int currentShadow = shadowDepth + (int)(2 * hoverProgress);
            DrawShadow(g, rect, radius, currentShadow);

            using var path = CreateRoundedRect(rect, radius);
            Color bgFill = LerpColor(SurfaceDark, SurfaceMid, hoverProgress);
            using var bg = new SolidBrush(bgFill);
            g.FillPath(bg, path);

            if (hoverProgress > 0.01f)
                PaintGlowBorder(g, rect, radius, AccentGlow, hoverProgress * 0.4f);

            int borderAlpha = 30 + (int)(30 * hoverProgress);
            using var border = new Pen(Color.FromArgb(borderAlpha, AccentGlow.R, AccentGlow.G, AccentGlow.B), 1f);
            g.DrawPath(border, path);

            // Gradient accent stripe at top
            var accentRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, 3);
            using var accentClip = CreateRoundedRect(rect, radius);
            var prevClip = g.Clip;
            g.SetClip(accentClip, CombineMode.Intersect);
            using var accentBrush = new LinearGradientBrush(accentRect, gradStart, gradEnd, 0f);
            g.FillRectangle(accentBrush, accentRect);
            g.Clip = prevClip;
        }

        /// <summary>
        /// Uses reflection to enable DoubleBuffered property on a Control to eliminate flickering.
        /// </summary>
        public static void EnableDoubleBuffering(Control control)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(control, true, null);
        }
    }
}
