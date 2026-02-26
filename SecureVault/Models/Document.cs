// ============================================
// SecureVault - Document Model
// Represents a document stored in the vault
// ============================================

namespace SecureVault.Models
{
    /// <summary>
    /// Represents a document stored in SecureVault.
    /// </summary>
    public class Document
    {
        public int DocumentID { get; set; }
        public int UserID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; } // Joined field
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsImportant { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public DateTime? LastViewedDate { get; set; }

        /// <summary>
        /// Returns human-readable file size string.
        /// </summary>
        public string FileSizeDisplay
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F1} KB";
                if (FileSize < 1024 * 1024 * 1024) return $"{FileSize / (1024.0 * 1024.0):F1} MB";
                return $"{FileSize / (1024.0 * 1024.0 * 1024.0):F2} GB";
            }
        }
    }
}
