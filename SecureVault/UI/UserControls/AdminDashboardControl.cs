// ============================================
// SecureVault - Admin Dashboard (Layout Refactored)
// ============================================

using SecureVault.BLL;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.UserControls
{
    public class AdminDashboardControl : UserControl
    {
        private readonly UserService _userService = new();
        private readonly DocumentService _docService = new();
        private static readonly Font EmojiFont = new("Segoe UI Emoji", 20);

        public AdminDashboardControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            Load += (s, e) => BuildDashboard();
        }

        private void BuildDashboard()
        {
            Controls.Clear();

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent };
            Controls.Add(scroll);

            var root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, ColumnCount = 1, RowCount = 4,
                BackColor = Color.Transparent, Padding = new Padding(20, 15, 20, 15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scroll.Controls.Add(root);

            int totalUsers = 0, totalDocs = 0; long totalStorage = 0; Models.User? mostActive = null;
            try { totalUsers = _userService.GetTotalUserCount(); totalDocs = _docService.GetTotalDocumentCount(); totalStorage = _docService.GetTotalStorageUsed(); mostActive = _userService.GetMostActiveUser(); } catch { }

            root.Controls.Add(new Label
            {
                Text = "⚡ Admin Dashboard", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 0, 0, 15)
            }, 0, 0);

            var cardsFlow = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
                BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 20)
            };
            cardsFlow.Controls.Add(CreateStatCard("👥", "Total Users", totalUsers, AppTheme.GradientStart, AppTheme.GradientEnd));
            cardsFlow.Controls.Add(CreateStatCard("📂", "Total Documents", totalDocs, AppTheme.AccentTeal, AppTheme.AccentCyan));
            cardsFlow.Controls.Add(CreateStatCard("💾", "Total Storage", -1, Color.FromArgb(245, 158, 11), Color.FromArgb(234, 88, 12), totalStorage));
            cardsFlow.Controls.Add(CreateStatCard("🏆", "Most Active", -2, AppTheme.AccentPink, Color.FromArgb(168, 85, 247), label: mostActive?.FullName ?? "N/A"));
            root.Controls.Add(cardsFlow, 0, 1);

            root.Controls.Add(new Label
            {
                Text = "📊 Document Distribution", Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Margin = new Padding(0, 5, 0, 10)
            }, 0, 2);

            try
            {
                var dist = _docService.GetCategoryDistribution();
                if (dist.Count > 0)
                {
                    var chart = new Panel
                    {
                        Size = new Size(700, 200), BackColor = Color.Transparent,
                        Margin = new Padding(0)
                    };
                    chart.Paint += (s, e) => PaintBarChart(e.Graphics, chart.ClientRectangle, dist);
                    root.Controls.Add(chart, 0, 3);
                }
                else
                {
                    root.Controls.Add(new Label { Text = "No documents yet.", Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextMuted, BackColor = Color.Transparent, AutoSize = true }, 0, 3);
                }
            }
            catch { }
        }

        private void PaintBarChart(Graphics g, Rectangle bounds, Dictionary<string, int> data)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (data.Count == 0) return;
            int maxVal = data.Values.Max(); if (maxVal == 0) maxVal = 1;
            int barWidth = Math.Min(80, (bounds.Width - 40) / data.Count - 10);
            int x = 30, chartBottom = bounds.Height - 30, chartHeight = chartBottom - 20;
            Color[] colors = { AppTheme.GradientStart, AppTheme.AccentTeal, AppTheme.AccentCyan, AppTheme.AccentPink, AppTheme.AccentGlow, AppTheme.Warning };

            int i = 0;
            foreach (var kvp in data)
            {
                int barH = (int)((double)kvp.Value / maxVal * chartHeight);
                var barRect = new Rectangle(x, chartBottom - barH, barWidth, barH);
                using var path = AppTheme.CreateRoundedRect(barRect, 4);
                using var brush = new SolidBrush(colors[i % colors.Length]);
                g.FillPath(brush, path);
                TextRenderer.DrawText(g, kvp.Value.ToString(), AppTheme.BodySmall, new Point(x + barWidth / 2 - 10, chartBottom - barH - 18), AppTheme.TextPrimary);
                TextRenderer.DrawText(g, kvp.Key.Length > 12 ? kvp.Key[..12] + "…" : kvp.Key, AppTheme.BodySmall, new Point(x, chartBottom + 5), AppTheme.TextSecondary);
                x += barWidth + 15; i++;
            }
        }

        private Panel CreateStatCard(string icon, string title, int value, Color gradStart, Color gradEnd, long storageBytes = -1, string? label = null)
        {
            bool isHovered = false;
            var card = new Panel { Size = new Size(260, 140), Margin = new Padding(0, 0, 15, 10), BackColor = Color.Transparent, Cursor = Cursors.Hand };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                AppTheme.DrawShadow(e.Graphics, rect, AppTheme.RadiusMedium, isHovered ? 7 : 5);
                using var path = AppTheme.CreateRoundedRect(rect, AppTheme.RadiusMedium);
                using var bg = new SolidBrush(isHovered ? AppTheme.SurfaceMid : AppTheme.SurfaceDark);
                e.Graphics.FillPath(bg, path);
                using var border = new Pen(isHovered ? Color.FromArgb(80, AppTheme.AccentGlow.R, AppTheme.AccentGlow.G, AppTheme.AccentGlow.B) : AppTheme.SurfaceBorder, isHovered ? 1.2f : 1f);
                e.Graphics.DrawPath(border, path);
                var accent = new Rectangle(0, 0, card.Width - 1, 4);
                using var ap = AppTheme.CreateRoundedRect(accent, 2);
                using var ab = new LinearGradientBrush(accent, gradStart, gradEnd, 0f);
                e.Graphics.FillPath(ab, ap);
            };

            void OnEnter(object? s, EventArgs e) { isHovered = true; card.Invalidate(); }
            void OnLeave(object? s, EventArgs e) { isHovered = false; card.Invalidate(); }
            card.MouseEnter += OnEnter;
            card.MouseLeave += OnLeave;

            // Inner layout using TableLayoutPanel — no hardcoded positions
            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(16, 14, 16, 10),
                Margin = new Padding(0)
            };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            inner.MouseEnter += OnEnter;
            inner.MouseLeave += OnLeave;

            var iconLabel = new Label
            {
                Text = icon, Font = EmojiFont,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = false, Margin = new Padding(0)
            };
            iconLabel.MouseEnter += OnEnter;
            iconLabel.MouseLeave += OnLeave;
            inner.Controls.Add(iconLabel, 0, 0);
            inner.SetRowSpan(iconLabel, 2);

            var valueLabel = new Label
            {
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = false, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                UseCompatibleTextRendering = false, Margin = new Padding(0)
            };
            valueLabel.MouseEnter += OnEnter;
            valueLabel.MouseLeave += OnLeave;
            inner.Controls.Add(valueLabel, 1, 0);

            var titleLabel = new Label
            {
                Text = title, Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent,
                AutoSize = false, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                UseCompatibleTextRendering = false, Margin = new Padding(0, 2, 0, 0)
            };
            titleLabel.MouseEnter += OnEnter;
            titleLabel.MouseLeave += OnLeave;
            inner.Controls.Add(titleLabel, 1, 1);

            card.Controls.Add(inner);

            if (label != null) valueLabel.Text = label;
            else if (storageBytes >= 0) AnimationHelper.AnimateStorageCounter(valueLabel, storageBytes, 800);
            else if (value >= 0) AnimationHelper.AnimateCounter(valueLabel, value, durationMs: 800);

            return card;
        }
    }
}
