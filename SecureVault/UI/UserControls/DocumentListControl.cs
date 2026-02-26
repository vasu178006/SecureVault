// ============================================
// SecureVault - Document List (Layout Refactored)
// TableLayoutPanel filter bar + dock-fill grid
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.Models;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class DocumentListControl : UserControl
    {
        private readonly DocumentService _docService = new();
        private readonly CategoryService _catService = new();
        private StyledDataGridView _grid = null!;
        private RoundedTextBox _searchBox = null!;
        private ComboBox _categoryFilter = null!;
        private ComboBox _typeFilter = null!;
        private ComboBox _sortFilter = null!;
        private Label _countLabel = null!;
        private List<Document> _documents = new();

        public DocumentListControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            DoubleBuffered = true;
            Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            // Root layout
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(15, 10, 15, 10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));   // Filter bar
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));   // Count label
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Grid
            Controls.Add(root);

            // ── Filter Bar (FlowLayout) ──
            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            root.Controls.Add(filterFlow, 0, 0);

            _searchBox = new RoundedTextBox
            {
                PlaceholderText = "🔍 Search documents...",
                Size = new Size(260, 40),
                Margin = new Padding(0, 7, 10, 0)
            };
            _searchBox.TextChanged += (s, e) => LoadDocuments();
            filterFlow.Controls.Add(_searchBox);

            _categoryFilter = CreateComboBox(150);
            filterFlow.Controls.Add(_categoryFilter);
            LoadCategories();

            _typeFilter = CreateComboBox(110);
            _typeFilter.Items.AddRange(new object[] { "All Types", ".pdf", ".jpg", ".png", ".docx", ".xlsx", ".txt" });
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += (s, e) => LoadDocuments();
            filterFlow.Controls.Add(_typeFilter);

            _sortFilter = CreateComboBox(130);
            _sortFilter.Items.AddRange(new object[] { "Sort by Date", "Sort by Name", "Sort by Size", "Sort by Type" });
            _sortFilter.SelectedIndex = 0;
            _sortFilter.SelectedIndexChanged += (s, e) => LoadDocuments();
            filterFlow.Controls.Add(_sortFilter);

            var exportBtn = new RoundedButton
            {
                Text = "📥 Export CSV",
                Size = new Size(120, 36),
                IsGradient = false,
                FlatColor = AppTheme.SurfaceMid,
                CornerRadius = 8,
                Margin = new Padding(10, 9, 0, 0)
            };
            exportBtn.Click += ExportButton_Click;
            filterFlow.Controls.Add(exportBtn);

            // ── Count label ──
            _countLabel = new Label
            {
                Text = "0 documents",
                Font = AppTheme.BodySmall,
                ForeColor = AppTheme.TextMuted,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(_countLabel, 0, 1);

            // ── DataGridView ──
            _grid = new StyledDataGridView { Dock = DockStyle.Fill };
            SetupGridColumns();
            SetupContextMenu();
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenSelectedFile(); };
            root.Controls.Add(_grid, 0, 2);

            LoadDocuments();
        }

        private void SetupGridColumns()
        {
            _grid.Columns.Add("Icon", "");
            _grid.Columns["Icon"].Width = 40;
            _grid.Columns["Icon"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("FileName", "File Name");
            _grid.Columns.Add("FileType", "Type");
            _grid.Columns["FileType"].Width = 70;
            _grid.Columns["FileType"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("FileSize", "Size");
            _grid.Columns["FileSize"].Width = 90;
            _grid.Columns["FileSize"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Category", "Category");
            _grid.Columns["Category"].Width = 120;
            _grid.Columns["Category"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Tags", "Tags");
            _grid.Columns["Tags"].Width = 130;
            _grid.Columns["Tags"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("UploadDate", "Uploaded");
            _grid.Columns["UploadDate"].Width = 130;
            _grid.Columns["UploadDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("Important", "⭐");
            _grid.Columns["Important"].Width = 40;
            _grid.Columns["Important"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void SetupContextMenu()
        {
            var ctx = new ContextMenuStrip();
            ctx.BackColor = AppTheme.SurfaceDark;
            ctx.ForeColor = AppTheme.TextPrimary;
            ctx.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            ctx.Items.Add("📂 Open File", null, (s, e) => OpenSelectedFile());
            ctx.Items.Add("✏️ Edit Details", null, (s, e) => EditSelectedDocument());
            ctx.Items.Add("⭐ Toggle Important", null, (s, e) => ToggleImportant());
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("🗑️ Delete", null, (s, e) => DeleteSelectedDocument());
            _grid.ContextMenuStrip = ctx;
        }

        private void LoadCategories()
        {
            _categoryFilter.Items.Clear();
            _categoryFilter.Items.Add("All Categories");
            try
            {
                var cats = _catService.GetCategories(SessionManager.CurrentUserID);
                foreach (var cat in cats) _categoryFilter.Items.Add(cat.CategoryName);
            }
            catch { }
            _categoryFilter.SelectedIndex = 0;
            _categoryFilter.SelectedIndexChanged += (s, e) => LoadDocuments();
        }

        private void LoadDocuments()
        {
            try
            {
                string? search = string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text;
                int? catId = null;
                if (_categoryFilter.SelectedIndex > 0)
                {
                    var cats = _catService.GetCategories(SessionManager.CurrentUserID);
                    var sel = cats.FirstOrDefault(c => c.CategoryName == _categoryFilter.SelectedItem?.ToString());
                    catId = sel?.CategoryID;
                }
                string? fileType = _typeFilter.SelectedIndex > 0 ? _typeFilter.SelectedItem?.ToString() : null;
                string? sortBy = _sortFilter.SelectedIndex switch { 1 => "name", 2 => "size", 3 => "type", _ => null };

                _documents = _docService.GetDocuments(SessionManager.CurrentUserID, search, catId, fileType, sortBy: sortBy);
                PopulateGrid();
            }
            catch (Exception ex) { _countLabel.Text = $"Error: {ex.Message}"; }
        }

        private void PopulateGrid()
        {
            _grid.Rows.Clear();
            foreach (var doc in _documents)
            {
                string icon = doc.FileType?.ToLower() switch
                {
                    ".pdf" => "📕", ".jpg" or ".jpeg" or ".png" => "🖼️",
                    ".docx" or ".doc" => "📘", ".xlsx" => "📗", _ => "📄"
                };
                _grid.Rows.Add(icon, doc.FileName, doc.FileType, doc.FileSizeDisplay,
                    doc.CategoryName ?? "—", doc.Tags ?? "—",
                    doc.UploadDate.ToString("MMM dd, yyyy HH:mm"),
                    doc.IsImportant ? "⭐" : "");
            }
            _countLabel.Text = $"{_documents.Count} document(s)";
        }

        private Document? GetSelectedDocument()
        {
            if (_grid.SelectedRows.Count == 0) return null;
            int idx = _grid.SelectedRows[0].Index;
            return idx < _documents.Count ? _documents[idx] : null;
        }

        private void OpenSelectedFile()
        {
            var doc = GetSelectedDocument(); if (doc == null) return;
            if (File.Exists(doc.FilePath))
            {
                _docService.MarkAsViewed(doc.DocumentID);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = doc.FilePath, UseShellExecute = true });
            }
            else MessageBox.Show("File not found on disk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void EditSelectedDocument()
        {
            var doc = GetSelectedDocument(); if (doc == null) return;
            var editForm = new EditDocumentForm(doc);
            if (editForm.ShowDialog() == DialogResult.OK) LoadDocuments();
        }

        private void ToggleImportant()
        {
            var doc = GetSelectedDocument(); if (doc == null) return;
            _docService.ToggleImportant(doc.DocumentID);
            LoadDocuments();
        }

        private void DeleteSelectedDocument()
        {
            var doc = GetSelectedDocument(); if (doc == null) return;
            if (MessageBox.Show($"Delete '{doc.FileName}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _docService.SoftDelete(doc.DocumentID, SessionManager.CurrentUserID);
                LoadDocuments();
            }
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            if (_documents.Count == 0) return;
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                FileName = $"SecureVault_Documents_{DateTime.Now:yyyyMMdd}.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, _docService.ExportToCsv(_documents));
                MessageBox.Show("Export complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private ComboBox CreateComboBox(int width) => new()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = AppTheme.SurfaceDark,
            ForeColor = AppTheme.TextPrimary,
            FlatStyle = FlatStyle.Flat,
            Font = AppTheme.BodyRegular,
            Size = new Size(width, 36),
            Margin = new Padding(0, 9, 10, 0)
        };
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => AppTheme.SurfaceLight;
        public override Color MenuBorder => AppTheme.SurfaceBorder;
        public override Color MenuItemBorder => AppTheme.AccentCyan;
        public override Color ToolStripDropDownBackground => AppTheme.SurfaceDark;
        public override Color ImageMarginGradientBegin => AppTheme.SurfaceDark;
        public override Color ImageMarginGradientMiddle => AppTheme.SurfaceDark;
        public override Color ImageMarginGradientEnd => AppTheme.SurfaceDark;
        public override Color SeparatorDark => AppTheme.SurfaceBorder;
        public override Color SeparatorLight => AppTheme.SurfaceBorder;
    }

    public class EditDocumentForm : Form
    {
        private readonly DocumentService _docService = new();
        private readonly CategoryService _catService = new();
        private readonly Document _doc;
        private RoundedTextBox _nameBox = null!;
        private RoundedTextBox _descBox = null!;
        private RoundedTextBox _tagsBox = null!;
        private ComboBox _catCombo = null!;
        private CheckBox _importantCheck = null!;
        private List<Category> _cats = new();

        public EditDocumentForm(Document doc)
        {
            _doc = doc;
            Text = "Edit Document";
            Size = new Size(460, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = AppTheme.PrimaryDark;
            AutoScaleMode = AutoScaleMode.None;
            BuildUI();
        }

        private void BuildUI()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 11,
                BackColor = Color.Transparent, Padding = new Padding(20, 15, 20, 15)
            };
            for (int i = 0; i < 5; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Buttons row
            Controls.Add(layout);

            int r = 0;
            layout.Controls.Add(MakeLabel("File Name"), 0, r++);
            _nameBox = new RoundedTextBox { Text = _doc.FileName, Dock = DockStyle.Fill };
            layout.Controls.Add(_nameBox, 0, r++);

            layout.Controls.Add(MakeLabel("Description"), 0, r++);
            _descBox = new RoundedTextBox { Text = _doc.Description ?? "", Dock = DockStyle.Fill };
            layout.Controls.Add(_descBox, 0, r++);

            layout.Controls.Add(MakeLabel("Tags (comma-separated)"), 0, r++);
            _tagsBox = new RoundedTextBox { Text = _doc.Tags ?? "", Dock = DockStyle.Fill };
            layout.Controls.Add(_tagsBox, 0, r++);

            layout.Controls.Add(MakeLabel("Category"), 0, r++);
            _catCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill,
                BackColor = AppTheme.SurfaceDark, ForeColor = AppTheme.TextPrimary,
                Font = AppTheme.BodyRegular
            };
            _catCombo.Items.Add("None");
            _cats = _catService.GetCategories(SessionManager.CurrentUserID);
            int selIdx = 0;
            for (int i = 0; i < _cats.Count; i++)
            {
                _catCombo.Items.Add(_cats[i].CategoryName);
                if (_doc.CategoryID.HasValue && _cats[i].CategoryID == _doc.CategoryID.Value) selIdx = i + 1;
            }
            _catCombo.SelectedIndex = selIdx;
            layout.Controls.Add(_catCombo, 0, r++);

            layout.Controls.Add(MakeLabel(""), 0, r++); // spacer for label row
            _importantCheck = new CheckBox
            {
                Text = "  Mark as Important", Checked = _doc.IsImportant,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Font = AppTheme.BodyRegular, Dock = DockStyle.Fill
            };
            layout.Controls.Add(_importantCheck, 0, r++);

            // Buttons row
            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var saveBtn = new RoundedButton { Text = "Save Changes", Size = new Size(180, 40), Margin = new Padding(0, 4, 10, 0) };
            saveBtn.Click += SaveClick;
            btnFlow.Controls.Add(saveBtn);
            var cancelBtn = new RoundedButton
            {
                Text = "Cancel", Size = new Size(120, 40), IsGradient = false,
                FlatColor = AppTheme.SurfaceMid, Margin = new Padding(0, 4, 0, 0)
            };
            cancelBtn.Click += (s, e) => Close();
            btnFlow.Controls.Add(cancelBtn);
            layout.Controls.Add(btnFlow, 0, r);
        }

        private void SaveClick(object? sender, EventArgs e)
        {
            _doc.FileName = _nameBox.Text.Trim();
            _doc.Description = _descBox.Text.Trim();
            _doc.Tags = _tagsBox.Text.Trim();
            _doc.IsImportant = _importantCheck.Checked;
            _doc.CategoryID = _catCombo.SelectedIndex > 0 ? _cats[_catCombo.SelectedIndex - 1].CategoryID : null;

            var (success, msg) = _docService.UpdateDocument(_doc);
            if (success) { DialogResult = DialogResult.OK; Close(); }
            else MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private Label MakeLabel(string text) => new()
        {
            Text = text, Font = AppTheme.BodySmall, ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft
        };
    }
}
