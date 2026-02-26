// ============================================
// SecureVault - Category Control (Layout Refactored)
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
                BackColor = Color.Transparent, Padding = new Padding(15, 10, 15, 10)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Manage Categories", Font = AppTheme.HeadingMedium,
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
                Size = new Size(300, 40), Margin = new Padding(0, 5, 10, 0)
            };
            addFlow.Controls.Add(_newCatBox);

            var addBtn = new RoundedButton
            {
                Text = "➕ Add Category", Size = new Size(160, 40),
                ButtonColorStart = AppTheme.AccentTeal, ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186), HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 5, 0, 0)
            };
            addBtn.Click += AddCategory_Click;
            addFlow.Controls.Add(addBtn);
            root.Controls.Add(addFlow, 0, 1);

            _grid = new StyledDataGridView { Dock = DockStyle.Fill };
            _grid.Columns.Add("Name", "Category Name");
            _grid.Columns.Add("Type", "Type");
            _grid.Columns["Type"].Width = 100; _grid.Columns["Type"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("DocCount", "Documents");
            _grid.Columns["DocCount"].Width = 100; _grid.Columns["DocCount"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

            var ctx = new ContextMenuStrip { BackColor = AppTheme.SurfaceDark, ForeColor = AppTheme.TextPrimary };
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
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        private void AddCategory_Click(object? sender, EventArgs e)
        {
            var (success, msg) = _catService.CreateCategory(_newCatBox.Text.Trim(), SessionManager.CurrentUserID);
            if (success) { _newCatBox.Text = ""; LoadCategories(); }
            else MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void DeleteCategory_Click(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;
            var cats = _catService.GetCategories(SessionManager.CurrentUserID);
            int idx = _grid.SelectedRows[0].Index;
            if (idx >= cats.Count) return;
            var cat = cats[idx];
            if (cat.IsSystemCategory) { MessageBox.Show("System categories cannot be deleted."); return; }
            if (MessageBox.Show($"Delete '{cat.CategoryName}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var (success, msg) = _catService.DeleteCategory(cat.CategoryID, SessionManager.CurrentUserID);
                if (success) LoadCategories(); else MessageBox.Show(msg);
            }
        }
    }
}
