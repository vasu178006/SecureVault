// ============================================
// SecureVault - Password Helper
// SHA256 password hashing utility
// ============================================

using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Helpers
{
    /// <summary>
    /// Provides password hashing and verification using SHA256.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a password string using SHA256.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>Uppercase hex string of the SHA256 hash.</returns>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="storedHash">The stored hash to compare against.</param>
        /// <returns>True if the password matches the hash.</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            string hash = HashPassword(password);
            return hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
