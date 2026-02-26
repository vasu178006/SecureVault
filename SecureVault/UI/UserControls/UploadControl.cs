// ============================================
// SecureVault - Upload Control (Layout Refactored)
// TableLayoutPanel structure with dock-fill drop zone
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;
using System.Drawing.Drawing2D;

namespace SecureVault.UI.UserControls
{
    public class UploadControl : UserControl
    {
        private readonly DocumentService _docService = new();
        private readonly CategoryService _catService = new();
        private StyledComboBox _categoryCombo = null!;
        private RoundedTextBox _descBox = null!;
        private RoundedTextBox _tagsBox = null!;
        private Label _statusLabel = null!;
        private ProgressBar _progressBar = null!;

        public UploadControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            AllowDrop = true;
            Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 15, 20, 15)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Title
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));  // Drop zone
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));  // Options
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));   // Browse + status
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));   // Progress
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Spacer
            Controls.Add(root);

            // Title
            root.Controls.Add(new Label
            {
                Text = "Upload Documents",
                Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // Drop zone
            var dropZone = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AllowDrop = true,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 0, 10)
            };
            dropZone.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(2, 2, dropZone.Width - 5, dropZone.Height - 5);
                using var path = AppTheme.CreateRoundedRect(rect, AppTheme.RadiusLarge);
                using var bg = new SolidBrush(Color.FromArgb(15, AppTheme.AccentCyan.R, AppTheme.AccentCyan.G, AppTheme.AccentCyan.B));
                e.Graphics.FillPath(bg, path);
                using var pen = new Pen(AppTheme.AccentCyan, 2f) { DashStyle = DashStyle.Dash };
                e.Graphics.DrawPath(pen, path);
            };
            var dropLabel = new Label
            {
                Text = "📤  Drag & Drop files here\nor click to browse",
                Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.AccentCyan,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            dropLabel.Click += BrowseFiles;
            dropZone.Controls.Add(dropLabel);

            // Drag-over feedback
            dropZone.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    e.Effect = DragDropEffects.Copy;
                    dropLabel.ForeColor = AppTheme.TextPrimary;
                    dropLabel.Text = "📥  Drop files to upload!";
                    dropZone.Invalidate();
                }
            };
            dropZone.DragLeave += (s, e) =>
            {
                dropLabel.ForeColor = AppTheme.AccentCyan;
                dropLabel.Text = "📤  Drag & Drop files here\nor click to browse";
                dropZone.Invalidate();
            };
            dropZone.DragDrop += async (s, e) => { if (e.Data?.GetData(DataFormats.FileDrop) is string[] files) await UploadFiles(files); };
            dropZone.Click += BrowseFiles;
            root.Controls.Add(dropZone, 0, 1);

            // Options: category, description, tags
            var optionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Labels row
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Category + Desc inputs
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // Tags label
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));  // Tags input

            optionsLayout.Controls.Add(new Label { Text = "Category", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            optionsLayout.Controls.Add(new Label { Text = "Description", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 0);

            _categoryCombo = new StyledComboBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 10, 2)
            };
            _categoryCombo.Items.Add("None");
            try { foreach (var cat in _catService.GetCategories(SessionManager.CurrentUserID)) _categoryCombo.Items.Add(cat.CategoryName); } catch { }
            _categoryCombo.SelectedIndex = 0;
            optionsLayout.Controls.Add(_categoryCombo, 0, 1);

            _descBox = new RoundedTextBox { PlaceholderText = "Optional description", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            optionsLayout.Controls.Add(_descBox, 1, 1);

            var tagsLabel = new Label { Text = "Tags (comma-separated)", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft };
            optionsLayout.Controls.Add(tagsLabel, 0, 2);
            optionsLayout.SetColumnSpan(tagsLabel, 2);

            _tagsBox = new RoundedTextBox { PlaceholderText = "Tags (e.g. passport, travel, 2024)", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            optionsLayout.Controls.Add(_tagsBox, 0, 3);
            optionsLayout.SetColumnSpan(_tagsBox, 2);
            root.Controls.Add(optionsLayout, 0, 2);

            // Browse + status row
            var browseFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var browseBtn = new RoundedButton
            {
                Text = "📁 Browse Files", Size = new Size(180, 44),
                Margin = new Padding(0, 4, 15, 0)
            };
            browseBtn.Click += BrowseFiles;
            browseFlow.Controls.Add(browseBtn);

            _statusLabel = new Label
            {
                Text = "", Font = AppTheme.BodyRegular, ForeColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent, AutoSize = true,
                Margin = new Padding(0, 15, 0, 0)
            };
            browseFlow.Controls.Add(_statusLabel);
            root.Controls.Add(browseFlow, 0, 3);

            // Progress bar
            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Top, Height = 8,
                Style = ProgressBarStyle.Continuous, Visible = false
            };
            root.Controls.Add(_progressBar, 0, 4);
        }

        private async void BrowseFiles(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Allowed Files|*.pdf;*.jpg;*.jpeg;*.png;*.docx;*.doc;*.xlsx;*.txt|All Files|*.*",
                Title = "Select files to upload"
            };
            if (ofd.ShowDialog() == DialogResult.OK) await UploadFiles(ofd.FileNames);
        }

        private async Task UploadFiles(string[] filePaths)
        {
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _progressBar.Maximum = filePaths.Length;

            int? catId = null;
            if (_categoryCombo.SelectedIndex > 0)
            {
                try
                {
                    var cats = _catService.GetCategories(SessionManager.CurrentUserID);
                    catId = cats[_categoryCombo.SelectedIndex - 1].CategoryID;
                }
                catch { }
            }

            string desc = string.IsNullOrWhiteSpace(_descBox.Text) ? null! : _descBox.Text.Trim();
            string tags = string.IsNullOrWhiteSpace(_tagsBox.Text) ? null! : _tagsBox.Text.Trim();
            int ok = 0, fail = 0;

            foreach (var fp in filePaths)
            {
                _statusLabel.Text = $"Uploading {Path.GetFileName(fp)}...";
                _statusLabel.ForeColor = AppTheme.AccentCyan;
                var (success, _, _) = await _docService.UploadAsync(SessionManager.CurrentUserID, fp, catId, desc, tags);
                if (success) ok++; else fail++;
                _progressBar.Value++;
                await Task.Delay(100);
            }

            _progressBar.Visible = false;
            _statusLabel.Text = $"✅ {ok} uploaded" + (fail > 0 ? $", ❌ {fail} failed" : "");
            _statusLabel.ForeColor = fail > 0 ? AppTheme.Warning : AppTheme.Success;
        }
    }
}
