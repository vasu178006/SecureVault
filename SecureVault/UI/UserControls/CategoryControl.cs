// ============================================
// SecureVault - Category Control (Redesigned)
// Toast/ModernDialog, consistent spacing
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class CategoryControl : UserControl
    {
        private readonly CategoryService _catService = new();
        private StyledDataGridView _grid = null!;
        private RoundedTextBox _newCatBox = null!;

        public CategoryControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PrimaryDark;
            Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 14)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "🏷️ Manage Categories", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var addFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            _newCatBox = new RoundedTextBox
            {
                PlaceholderText = "New category name...",
                Size = new Size(300, 42), Margin = new Padding(0, 6, 10, 0)
            };
            addFlow.Controls.Add(_newCatBox);

            var addBtn = new RoundedButton
            {
                Text = "➕ Add Category", Size = new Size(160, 42),
                ButtonColorStart = AppTheme.AccentTeal, ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186), HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 6, 0, 0)
            };
            addBtn.Click += AddCategory_Click;
            addFlow.Controls.Add(addBtn);
            root.Controls.Add(addFlow, 0, 1);

            _grid = new StyledDataGridView
            {
                Dock = DockStyle.Fill,
                EmptyMessage = "No categories created yet"
            };
            _grid.Columns.Add("Name", "Category Name");
            _grid.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _grid.Columns.Add("Type", "Type");
            _grid.Columns["Type"].Width = 100;
            _grid.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("DocCount", "Documents");
            _grid.Columns["DocCount"].Width = 120;
            _grid.Columns["DocCount"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            var ctx = new ContextMenuStrip { BackColor = AppTheme.SurfaceDark, ForeColor = AppTheme.TextPrimary };
            ctx.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            ctx.Items.Add("🗑️ Delete Category", null, DeleteCategory_Click);
            _grid.ContextMenuStrip = ctx;
            root.Controls.Add(_grid, 0, 2);

            LoadCategories();
        }

        private void LoadCategories()
        {
            _grid.Rows.Clear();
            try
            {
                foreach (var cat in _catService.GetCategories(SessionManager.CurrentUserID))
                    _grid.Rows.Add(cat.CategoryName, cat.IsSystemCategory ? "System" : "Custom", cat.DocumentCount);
            }
            catch (Exception ex) { ToastNotification.Show($"Error: {ex.Message}", ToastType.Error); }
        }

        private void AddCategory_Click(object? sender, EventArgs e)
        {
            var (success, msg) = _catService.CreateCategory(_newCatBox.Text.Trim(), SessionManager.CurrentUserID);
            if (success)
            {
                _newCatBox.Text = "";
                LoadCategories();
                ToastNotification.Show("Category created!", ToastType.Success);
            }
            else ToastNotification.Show(msg, ToastType.Error);
        }

        private void DeleteCategory_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;
            var cats = _catService.GetCategories(SessionManager.CurrentUserID);
            int idx = _grid.SelectedRows[0].Index;
            if (idx >= cats.Count) return;
            var cat = cats[idx];
            if (cat.IsSystemCategory)
            {
                ToastNotification.Show("System categories cannot be deleted.", ToastType.Warning);
                return;
            }
            if (ModernDialog.ConfirmDelete("Delete Category",
                $"Delete '{cat.CategoryName}'? Documents in this category will become uncategorized.",
                "Delete", FindForm()))
            {
                var (success, msg) = _catService.DeleteCategory(cat.CategoryID, SessionManager.CurrentUserID);
                if (success)
                {
                    LoadCategories();
                    ToastNotification.Show("Category deleted", ToastType.Info);
                }
                else ToastNotification.Show(msg, ToastType.Error);
            }
        }
    }
}
