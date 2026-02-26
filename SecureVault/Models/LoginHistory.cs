// ============================================
// SecureVault - LoginHistory Model
// ============================================

namespace SecureVault.Models
{
    /// <summary>
    /// Represents a login attempt record.
    /// </summary>
    public class LoginHistory
    {
        public int LoginID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; } // Joined field
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Success"; // "Success" or "Failed"
    }
}
