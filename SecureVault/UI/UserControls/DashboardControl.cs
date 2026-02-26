// ============================================
// SecureVault - Dashboard (Redesigned)
// Staggered card entrance, refined stat cards,
// animated counters, premium doc rows
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.UserControls
{
    public class DashboardControl : UserControl
    {
        private readonly DocumentService _docService = new();
        private static readonly Font EmojiFont = new("Segoe UI Emoji", 20);
        private static readonly Font EmojiSmallFont = new("Segoe UI Emoji", 14);

        public DashboardControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            Load += (s, e) => BuildDashboard();
        }

        private void BuildDashboard()
        {
            Controls.Clear();
            int userId = SessionManager.CurrentUserID;

            var scroll = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.Transparent
            };
            Controls.Add(scroll);

            var root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top, ColumnCount = 1, RowCount = 6,
                BackColor = Color.Transparent, Padding = new Padding(24, 20, 24, 20)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scroll.Controls.Add(root);

            int row = 0;

            // ── Welcome with subtle gradient text ──
            var welcomeLabel = new Label
            {
                Text = $"Welcome back, {SessionManager.CurrentUser?.FullName ?? "User"} 👋",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            root.Controls.Add(welcomeLabel, 0, row++);

            // ── Stats Cards ──
            int docCount = 0, importantCount = 0;
            long storageUsed = 0;
            try
            {
                docCount = _docService.GetDocumentCount(userId);
                importantCount = _docService.GetImportant(userId).Count;
                storageUsed = _docService.GetStorageUsed(userId);
            }
            catch { }

            var cardsFlow = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
                BackColor = Color.Transparent, Margin = new Padding(0, 0, 0, 28)
            };
            cardsFlow.Controls.Add(CreateStatCard("📂", "Total Documents", docCount,
                AppTheme.GradientStart, AppTheme.GradientEnd));
            cardsFlow.Controls.Add(CreateStatCard("💾", "Storage Used", -1,
                AppTheme.AccentTeal, AppTheme.AccentCyan, storageUsed));
            cardsFlow.Controls.Add(CreateStatCard("⭐", "Important", importantCount,
                AppTheme.AccentPink, Color.FromArgb(168, 85, 247)));
            cardsFlow.Controls.Add(CreateStatCard("📅", "This Month", 0,
                Color.FromArgb(245, 158, 11), Color.FromArgb(234, 88, 12)));
            root.Controls.Add(cardsFlow, 0, row++);

            // ── Recent Uploads ──
            root.Controls.Add(MakeSectionTitle("📄 Recent Uploads"), 0, row++);
            var recentFlow = CreateDocSection(userId, isRecent: true);
            root.Controls.Add(recentFlow, 0, row++);

            // ── Important Documents ──
            root.Controls.Add(MakeSectionTitle("⭐ Important Documents"), 0, row++);
            var impFlow = CreateDocSection(userId, isRecent: false);
            root.Controls.Add(impFlow, 0, row);
        }

        private Label MakeSectionTitle(string text) => new()
        {
            Text = text,
            Font = AppTheme.HeadingSmall,
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 12)
        };

        private FlowLayoutPanel CreateDocSection(int userId, bool isRecent)
        {
            var flow = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown, WrapContents = false,
                BackColor = Color.Transparent, Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 20)
            };
            try
            {
                var docs = isRecent
                    ? _docService.GetRecent(userId, 5)
                    : _docService.GetImportant(userId);
                if (docs.Count == 0)
                {
                    flow.Controls.Add(new Label
                    {
                        Text = isRecent
                            ? "No documents uploaded yet. Click 'Upload Files' to get started!"
                            : "No important documents marked.",
                        Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextMuted,
                        BackColor = Color.Transparent, AutoSize = true,
                        Margin = new Padding(0, 0, 0, 4)
                    });
                }
                else
                {
                    foreach (var doc in (isRecent ? docs : docs.Take(5)))
                        flow.Controls.Add(CreateDocRow(doc, ClientSize.Width - 100));
                }
            }
            catch { }
            return flow;
        }

        private Panel CreateStatCard(string icon, string title, int value,
            Color gradStart, Color gradEnd, long storageBytes = -1)
        {
            bool isHovered = false;
            var card = new Panel
            {
                Size = new Size(260, 130),
                Margin = new Padding(0, 0, 16, 12),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) =>
                AppTheme.PaintStatCard(e.Graphics,
                    new Rectangle(0, 0, card.Width - 1, card.Height - 1),
                    AppTheme.RadiusCard, gradStart, gradEnd, isHovered);

            void OnEnter(object? s, EventArgs e) { isHovered = true; card.Invalidate(); }
            void OnLeave(object? s, EventArgs e) { isHovered = false; card.Invalidate(); }
            card.MouseEnter += OnEnter;
            card.MouseLeave += OnLeave;

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(18, 18, 18, 12), Margin = new Padding(0)
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
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
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

            if (storageBytes >= 0)
                AnimationHelper.AnimateStorageCounter(valueLabel, storageBytes, 800);
            else if (value >= 0)
                AnimationHelper.AnimateCounter(valueLabel, value, durationMs: 800);

            return card;
        }

        private Panel CreateDocRow(Models.Document doc, int width)
        {
            if (width < 200) width = 600;
            bool isHovered = false;
            var row = new Panel
            {
                Size = new Size(width, 52),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };
            row.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, row.Width - 1, row.Height - 1);
                using var path = AppTheme.CreateRoundedRect(rect, 8);
                var bgColor = isHovered ? AppTheme.SurfaceMid : AppTheme.SurfaceDark;
                using var brush = new SolidBrush(bgColor);
                g.FillPath(brush, path);
                if (isHovered)
                {
                    using var borderPen = new Pen(Color.FromArgb(25, 255, 255, 255), 1f);
                    g.DrawPath(borderPen, path);
                }
            };

            string fileIcon = doc.FileType?.ToLower() switch
            {
                ".pdf" => "📕", ".jpg" or ".jpeg" or ".png" => "🖼️",
                ".docx" or ".doc" => "📘", ".xlsx" => "📗", _ => "📄"
            };

            row.Controls.Add(new Label { Text = fileIcon, Font = EmojiSmallFont,
                BackColor = Color.Transparent, AutoSize = false,
                Size = new Size(32, 32), Location = new Point(14, 10),
                TextAlign = ContentAlignment.MiddleCenter });
            row.Controls.Add(new Label { Text = doc.FileName,
                Font = new Font(AppTheme.FontFamily, 10.5f), ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(54, 6) });
            row.Controls.Add(new Label
            {
                Text = $"{doc.FileSizeDisplay}  ·  {doc.UploadDate:MMM dd, yyyy}  ·  {doc.CategoryName ?? "Uncategorized"}",
                Font = AppTheme.BodySmall, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(54, 28)
            });

            void Enter(object? s, EventArgs e) { isHovered = true; row.Invalidate(); }
            void Leave(object? s, EventArgs e) { isHovered = false; row.Invalidate(); }
            row.MouseEnter += Enter;
            row.MouseLeave += Leave;
            foreach (Control c in row.Controls)
            {
                c.MouseEnter += Enter;
                c.MouseLeave += Leave;
            }
            return row;
        }
    }
}
