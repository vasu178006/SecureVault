// ============================================
// SecureVault - Category Model
// ============================================

namespace SecureVault.Models
{
    /// <summary>
    /// Represents a document category (system-wide or user-specific).
    /// </summary>
    public class Category
    {
        public int CategoryID { get; set; }
        public int? UserID { get; set; }  // null = system category
        public string CategoryName { get; set; } = string.Empty;
        public int DocumentCount { get; set; } // computed field

        /// <summary>
        /// True if this is a system-default category (not user-created).
        /// </summary>
        public bool IsSystemCategory => UserID == null;
    }
}
