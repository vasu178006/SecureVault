// ============================================
// SecureVault - Recycle Bin (Redesigned)
// Toast/ModernDialog, consistent spacing
// ============================================

using SecureVault.BLL;
using SecureVault.Helpers;
using SecureVault.Models;
using SecureVault.UI.Controls;
using SecureVault.UI.Theme;

namespace SecureVault.UI.UserControls
{
    public class RecycleBinControl : UserControl
    {
        private readonly DocumentService _docService = new();
        private StyledDataGridView _grid = null!;
        private List<Document> _deletedDocs = new();

        public RecycleBinControl()
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
                Text = "🗑️ Recycle Bin", Font = AppTheme.HeadingMedium,
                ForeColor = AppTheme.TextPrimary, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            var restoreBtn = new RoundedButton
            {
                Text = "♻️ Restore", Size = new Size(140, 42),
                ButtonColorStart = AppTheme.AccentTeal, ButtonColorEnd = AppTheme.AccentCyan,
                HoverColorStart = Color.FromArgb(40, 204, 186), HoverColorEnd = Color.FromArgb(26, 202, 232),
                Margin = new Padding(0, 6, 10, 0)
            };
            restoreBtn.Click += RestoreSelected;
            btnFlow.Controls.Add(restoreBtn);

            if (SessionManager.IsAdmin)
            {
                var permBtn = new RoundedButton
                {
                    Text = "❌ Permanent Delete", Size = new Size(190, 42),
                    ButtonColorStart = AppTheme.Error, ButtonColorEnd = Color.FromArgb(185, 28, 28),
                    HoverColorStart = Color.FromArgb(255, 88, 88), HoverColorEnd = Color.FromArgb(205, 48, 48),
                    Margin = new Padding(0, 6, 0, 0)
                };
                permBtn.Click += PermanentDeleteSelected;
                btnFlow.Controls.Add(permBtn);
            }
            root.Controls.Add(btnFlow, 0, 1);

            _grid = new StyledDataGridView
            {
                Dock = DockStyle.Fill,
                EmptyMessage = "Recycle bin is empty"
            };
            _grid.Columns.Add("FileName", "File Name");
            _grid.Columns.Add("FileType", "Type");
            _grid.Columns["FileType"].Width = 80; _grid.Columns["FileType"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("FileSize", "Size");
            _grid.Columns["FileSize"].Width = 90; _grid.Columns["FileSize"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            _grid.Columns.Add("UploadDate", "Uploaded");
            _grid.Columns["UploadDate"].Width = 150; _grid.Columns["UploadDate"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            root.Controls.Add(_grid, 0, 2);

            LoadDeletedDocs();
        }

        private void LoadDeletedDocs()
        {
            _grid.Rows.Clear();
            try
            {
                _deletedDocs = _docService.GetDeleted(SessionManager.CurrentUserID);
                foreach (var doc in _deletedDocs)
                    _grid.Rows.Add(doc.FileName, doc.FileType, doc.FileSizeDisplay,
                        doc.UploadDate.ToString("MMM dd, yyyy HH:mm"));
            }
            catch { }
        }

        private void RestoreSelected(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;
            int idx = _grid.SelectedRows[0].Index;
            if (idx >= _deletedDocs.Count) return;
            var (success, msg) = _docService.Restore(_deletedDocs[idx].DocumentID, SessionManager.CurrentUserID);
            if (success)
            {
                LoadDeletedDocs();
                ToastNotification.Show("Document restored!", ToastType.Success);
            }
            else ToastNotification.Show(msg, ToastType.Error);
        }

        private void PermanentDeleteSelected(object? sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) return;
            int idx = _grid.SelectedRows[0].Index;
            if (idx >= _deletedDocs.Count) return;
            var doc = _deletedDocs[idx];

            if (ModernDialog.ConfirmDelete("Permanent Delete",
                $"Permanently delete '{doc.FileName}'?\nThis action cannot be undone!",
                "Delete Forever", FindForm()))
            {
                var (success, msg) = _docService.PermanentDelete(doc.DocumentID, SessionManager.CurrentUserID);
                if (success)
                {
                    LoadDeletedDocs();
                    ToastNotification.Show("File permanently deleted", ToastType.Info);
                }
                else ToastNotification.Show(msg, ToastType.Error);
            }
        }
    }
}
