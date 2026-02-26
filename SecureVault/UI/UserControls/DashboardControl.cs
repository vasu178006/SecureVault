// ============================================
// SecureVault - Dashboard (Layout Refactored)
// TableLayoutPanel + FlowLayoutPanel structure
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

            // Root: scrollable panel
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            Controls.Add(scroll);

            // Main vertical layout
            var root = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 15, 20, 15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Welcome
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Cards
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Recent title
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Recent list
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Important title
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Important list
            scroll.Controls.Add(root);

            int row = 0;

            // ── Welcome ──
            root.Controls.Add(new Label
            {
                Text = $"Welcome back, {SessionManager.CurrentUser?.FullName ?? "User"} 👋",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            }, 0, row++);

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
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 20)
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
            root.Controls.Add(new Label
            {
                Text = "📄 Recent Uploads",
                Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            }, 0, row++);

            var recentFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 15),
                Dock = DockStyle.Top
            };
            try
            {
                var recentDocs = _docService.GetRecent(userId, 5);
                if (recentDocs.Count == 0)
                {
                    recentFlow.Controls.Add(new Label
                    {
                        Text = "No documents uploaded yet. Click 'Upload Files' to get started!",
                        Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextMuted,
                        BackColor = Color.Transparent, AutoSize = true,
                        Margin = new Padding(0, 0, 0, 5)
                    });
                }
                else
                {
                    foreach (var doc in recentDocs)
                        recentFlow.Controls.Add(CreateDocRow(doc, scroll.Width - 80));
                }
            }
            catch { }
            root.Controls.Add(recentFlow, 0, row++);

            // ── Important Documents ──
            root.Controls.Add(new Label
            {
                Text = "⭐ Important Documents",
                Font = AppTheme.HeadingSmall,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            }, 0, row++);

            var impFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Dock = DockStyle.Top
            };
            try
            {
                var importantDocs = _docService.GetImportant(userId);
                if (importantDocs.Count == 0)
                {
                    impFlow.Controls.Add(new Label
                    {
                        Text = "No important documents marked.",
                        Font = AppTheme.BodyLarge, ForeColor = AppTheme.TextMuted,
                        BackColor = Color.Transparent, AutoSize = true
                    });
                }
                else
                {
                    foreach (var doc in importantDocs.Take(5))
                        impFlow.Controls.Add(CreateDocRow(doc, scroll.Width - 80));
                }
            }
            catch { }
            root.Controls.Add(impFlow, 0, row);
        }

        private Panel CreateStatCard(string icon, string title, int value,
            Color gradStart, Color gradEnd, long storageBytes = -1)
        {
            var card = new Panel
            {
                Size = new Size(240, 120),
                Margin = new Padding(0, 0, 15, 10),
                BackColor = Color.Transparent
            };

            var valueLabel = new Label
            {
                Font = AppTheme.HeadingLarge,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(70, 40)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                AppTheme.DrawShadow(e.Graphics, rect, AppTheme.RadiusMedium, 5);
                using var path = AppTheme.CreateRoundedRect(rect, AppTheme.RadiusMedium);
                using var bg = new SolidBrush(AppTheme.SurfaceDark);
                e.Graphics.FillPath(bg, path);
                using var border = new Pen(Color.FromArgb(30, 255, 255, 255), 1f);
                e.Graphics.DrawPath(border, path);
                var accent = new Rectangle(0, 0, card.Width - 1, 4);
                using var ap = AppTheme.CreateRoundedRect(accent, 2);
                using var ab = new LinearGradientBrush(accent, gradStart, gradEnd, 0f);
                e.Graphics.FillPath(ab, ap);
            };

            card.Controls.Add(new Label
            {
                Text = icon, Font = new Font("Segoe UI Emoji", 20),
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Location = new Point(18, 35)
            });
            card.Controls.Add(valueLabel);
            card.Controls.Add(new Label
            {
                Text = title, Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent,
                AutoSize = true, Location = new Point(70, 75)
            });

            if (storageBytes >= 0)
                AnimationHelper.AnimateStorageCounter(valueLabel, storageBytes, 800);
            else if (value >= 0)
                AnimationHelper.AnimateCounter(valueLabel, value, durationMs: 800);

            return card;
        }

        private Panel CreateDocRow(Models.Document doc, int width)
        {
            if (width < 200) width = 600;
            var row = new Panel
            {
                Size = new Size(width, 48),
                BackColor = AppTheme.SurfaceDark,
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };
            row.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = AppTheme.CreateRoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 6);
                using var brush = new SolidBrush(row.BackColor);
                e.Graphics.FillPath(brush, path);
            };

            string fileIcon = doc.FileType?.ToLower() switch
            {
                ".pdf" => "📕", ".jpg" or ".jpeg" or ".png" => "🖼️",
                ".docx" or ".doc" => "📘", ".xlsx" => "📗", _ => "📄"
            };

            row.Controls.Add(new Label { Text = fileIcon, Font = new Font("Segoe UI Emoji", 14),
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(12, 8) });
            row.Controls.Add(new Label { Text = doc.FileName,
                Font = new Font(AppTheme.FontFamily, 10), ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(48, 5) });
            row.Controls.Add(new Label
            {
                Text = $"{doc.FileSizeDisplay}  •  {doc.UploadDate:MMM dd, yyyy}  •  {doc.CategoryName ?? "Uncategorized"}",
                Font = AppTheme.BodySmall, ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent, AutoSize = true, Location = new Point(48, 26)
            });

            row.MouseEnter += (s, e) => row.BackColor = AppTheme.SurfaceLight;
            row.MouseLeave += (s, e) => row.BackColor = AppTheme.SurfaceDark;
            foreach (Control c in row.Controls)
            {
                c.MouseEnter += (s, e) => row.BackColor = AppTheme.SurfaceLight;
                c.MouseLeave += (s, e) => row.BackColor = AppTheme.SurfaceDark;
            }
            return row;
        }
    }
}
