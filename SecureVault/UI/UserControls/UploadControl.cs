// ============================================
// SecureVault - Upload Control (Redesigned)
// Animated drop zone, modern progress bar,
// toast notifications for feedback
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
        private ModernProgressBar _progressBar = null!;
        private bool _isDragOver;
        private Panel _dropZone = null!;
        private Label _dropLabel = null!;

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
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6,
                BackColor = Color.Transparent, Padding = new Padding(24, 20, 24, 20)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));    // Title
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));   // Drop zone
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));   // Options
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));    // Browse + status
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));    // Progress
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // Spacer
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
            _dropZone = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.Transparent,
                AllowDrop = true, Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 0, 12)
            };
            _dropZone.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(2, 2, _dropZone.Width - 5, _dropZone.Height - 5);
                using var path = AppTheme.CreateRoundedRect(rect, AppTheme.RadiusLarge);

                Color bgTint = _isDragOver
                    ? Color.FromArgb(25, AppTheme.AccentCyan.R, AppTheme.AccentCyan.G, AppTheme.AccentCyan.B)
                    : Color.FromArgb(10, AppTheme.AccentCyan.R, AppTheme.AccentCyan.G, AppTheme.AccentCyan.B);
                using var bg = new SolidBrush(bgTint);
                e.Graphics.FillPath(bg, path);

                float borderWidth = _isDragOver ? 2.5f : 1.5f;
                Color borderColor = _isDragOver ? AppTheme.AccentCyan : Color.FromArgb(80, AppTheme.AccentCyan.R, AppTheme.AccentCyan.G, AppTheme.AccentCyan.B);
                using var pen = new Pen(borderColor, borderWidth) { DashStyle = DashStyle.Dash };
                e.Graphics.DrawPath(pen, path);

                if (_isDragOver)
                    AppTheme.PaintGlowBorder(e.Graphics, rect, AppTheme.RadiusLarge, AppTheme.AccentCyan, 0.3f);
            };
            _dropLabel = new Label
            {
                Text = "📤  Drag & Drop files here\nor click to browse",
                Font = AppTheme.BodyLarge,
                ForeColor = AppTheme.AccentCyan,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _dropLabel.Click += BrowseFiles;
            _dropZone.Controls.Add(_dropLabel);

            _dropZone.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    e.Effect = DragDropEffects.Copy;
                    _isDragOver = true;
                    _dropLabel.ForeColor = AppTheme.TextPrimary;
                    _dropLabel.Text = "📥  Drop files to upload!";
                    _dropZone.Invalidate();
                }
            };
            _dropZone.DragLeave += (s, e) =>
            {
                _isDragOver = false;
                _dropLabel.ForeColor = AppTheme.AccentCyan;
                _dropLabel.Text = "📤  Drag & Drop files here\nor click to browse";
                _dropZone.Invalidate();
            };
            _dropZone.DragDrop += async (s, e) =>
            {
                _isDragOver = false;
                _dropZone.Invalidate();
                if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
                    await UploadFiles(files);
            };
            _dropZone.Click += BrowseFiles;
            root.Controls.Add(_dropZone, 0, 1);

            // Options
            var optionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4,
                BackColor = Color.Transparent
            };
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            optionsLayout.Controls.Add(new Label { Text = "Category", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 0, 0);
            optionsLayout.Controls.Add(new Label { Text = "Description", Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary, BackColor = Color.Transparent, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft }, 1, 0);

            _categoryCombo = new StyledComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 10, 2) };
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

            // Browse + status
            var browseFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var browseBtn = new RoundedButton
            {
                Text = "📁 Browse Files", Size = new Size(180, 44),
                Margin = new Padding(0, 4, 16, 0)
            };
            browseBtn.Click += BrowseFiles;
            browseFlow.Controls.Add(browseBtn);

            _statusLabel = new Label
            {
                Text = "", Font = AppTheme.BodyRegular, ForeColor = AppTheme.AccentTeal,
                BackColor = Color.Transparent, AutoSize = true,
                Margin = new Padding(0, 16, 0, 0)
            };
            browseFlow.Controls.Add(_statusLabel);
            root.Controls.Add(browseFlow, 0, 3);

            // Progress bar
            _progressBar = new ModernProgressBar
            {
                Dock = DockStyle.Top, Height = 8, Visible = false
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
            _statusLabel.Text = "";

            if (fail > 0)
                ToastNotification.Show($"{ok} uploaded, {fail} failed", ToastType.Warning);
            else
                ToastNotification.Show($"✅ {ok} file(s) uploaded successfully!", ToastType.Success);
        }
    }
}
