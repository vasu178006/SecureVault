// ============================================
// SecureVault - Validation Helper
// Input validation utilities
// ============================================

using System.Text.RegularExpressions;

namespace SecureVault.Helpers
{
    /// <summary>
    /// Provides input validation methods for the application.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates an email address format.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Validates password strength: min 6 chars, at least 1 uppercase, 1 lowercase, 1 digit.
        /// </summary>
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6) return false;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            return hasUpper && hasLower && hasDigit;
        }

        /// <summary>
        /// Validates that a name is not empty and is a reasonable length.
        /// </summary>
        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 100;
        }

        /// <summary>
        /// Checks if a file extension is in the allowed list.
        /// </summary>
        public static bool IsAllowedFileType(string fileName, string allowedTypes)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(allowedTypes))
                return false;

            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            var allowed = allowedTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim().ToLowerInvariant())
                                      .ToList();
            return allowed.Contains(ext);
        }

        /// <summary>
        /// Checks if a file size is within the allowed limit.
        /// </summary>
        /// <param name="fileSizeBytes">File size in bytes.</param>
        /// <param name="maxSizeMB">Maximum allowed size in megabytes.</param>
        public static bool IsWithinSizeLimit(long fileSizeBytes, int maxSizeMB)
        {
            return fileSizeBytes <= (long)maxSizeMB * 1024 * 1024;
        }

        /// <summary>
        /// Sanitizes a file name to remove invalid characters.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}
