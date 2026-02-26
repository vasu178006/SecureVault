// ============================================
// SecureVault - User Model
// Represents a user in the system
// ============================================

namespace SecureVault.Models
{
    /// <summary>
    /// Represents a user account in the SecureVault system.
    /// </summary>
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "Admin" or "User"
        public string? ProfileImagePath { get; set; }
        public bool IsBlocked { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Returns true if the user is an Admin.
        /// </summary>
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }
}
