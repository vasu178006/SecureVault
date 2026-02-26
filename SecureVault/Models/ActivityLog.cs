// ============================================
// SecureVault - ActivityLog Model
// ============================================

namespace SecureVault.Models
{
    /// <summary>
    /// Represents an activity log entry for audit tracking.
    /// </summary>
    public class ActivityLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; } // Joined field
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
