// ============================================
// SecureVault - User Service
// Business logic for user management
// ============================================

using SecureVault.DAL;
using SecureVault.Helpers;
using SecureVault.Models;

namespace SecureVault.BLL
{
    /// <summary>
    /// Handles user profile management and admin user operations.
    /// </summary>
    public class UserService
    {
        private readonly UserRepository _userRepo = new();
        private readonly ActivityLogRepository _activityLogRepo = new();

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        public User? GetById(int userId) => _userRepo.GetById(userId);

        /// <summary>
        /// Gets all users (optionally filtered by search term).
        /// </summary>
        public List<User> GetAll(string? searchTerm = null) => _userRepo.GetAll(searchTerm);

        /// <summary>
        /// Updates user profile information.
        /// </summary>
        public (bool Success, string Message) UpdateProfile(int userId, string fullName, string email, string? profileImagePath)
        {
            if (!ValidationHelper.IsValidName(fullName))
                return (false, "Name must be between 2 and 100 characters.");

            if (!ValidationHelper.IsValidEmail(email))
                return (false, "Please enter a valid email address.");

            var user = _userRepo.GetById(userId);
            if (user == null)
                return (false, "User not found.");

            // Check if email changed and already exists
            if (!user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = _userRepo.GetByEmail(email);
                if (existing != null)
                    return (false, "This email is already in use by another account.");
            }

            try
            {
                user.FullName = fullName;
                user.Email = email;
                user.ProfileImagePath = profileImagePath ?? user.ProfileImagePath;
                _userRepo.UpdateProfile(user);
                _activityLogRepo.Create(userId, "ProfileUpdate", "Profile updated");

                // Update session if current user
                if (SessionManager.CurrentUserID == userId)
                {
                    SessionManager.CurrentUser = user;
                }

                return (true, "Profile updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Blocks a user account (admin only).
        /// </summary>
        public (bool Success, string Message) BlockUser(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return (false, "User not found.");
            if (user.IsAdmin) return (false, "Cannot block an admin account.");

            try
            {
                _userRepo.SetBlocked(userId, true);
                _activityLogRepo.Create(SessionManager.CurrentUserID, "BlockUser",
                    $"Blocked user: {user.FullName} ({user.Email})");
                return (true, $"User {user.FullName} has been blocked.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to block user: {ex.Message}");
            }
        }

        /// <summary>
        /// Unblocks a user account (admin only).
        /// </summary>
        public (bool Success, string Message) UnblockUser(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return (false, "User not found.");

            try
            {
                _userRepo.SetBlocked(userId, false);
                _activityLogRepo.Create(SessionManager.CurrentUserID, "UnblockUser",
                    $"Unblocked user: {user.FullName} ({user.Email})");
                return (true, $"User {user.FullName} has been unblocked.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to unblock user: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user account (admin only).
        /// </summary>
        public (bool Success, string Message) DeleteUser(int userId)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return (false, "User not found.");
            if (user.IsAdmin) return (false, "Cannot delete an admin account.");

            try
            {
                _userRepo.Delete(userId);
                _activityLogRepo.Create(SessionManager.CurrentUserID, "DeleteUser",
                    $"Deleted user: {user.FullName} ({user.Email})");
                return (true, $"User {user.FullName} has been permanently deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete user: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets total user count (excluding admins).
        /// </summary>
        public int GetTotalUserCount() => _userRepo.GetTotalUserCount();

        /// <summary>
        /// Gets the most active user.
        /// </summary>
        public User? GetMostActiveUser() => _userRepo.GetMostActiveUser();
    }
}
