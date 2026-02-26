// ============================================
// SecureVault - Auth Service
// Business logic for authentication
// ============================================

using SecureVault.DAL;
using SecureVault.Helpers;
using SecureVault.Models;
using System.Configuration;

namespace SecureVault.BLL
{
    /// <summary>
    /// Handles user authentication, registration, and password management.
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _userRepo = new();
        private readonly LoginHistoryRepository _loginHistoryRepo = new();
        private readonly ActivityLogRepository _activityLogRepo = new();

        private int MaxFailedAttempts
        {
            get
            {
                string? val = ConfigurationManager.AppSettings["MaxFailedAttempts"];
                return int.TryParse(val, out int max) ? max : 3;
            }
        }

        /// <summary>
        /// Registers a new user. Returns (success, errorMessage).
        /// </summary>
        public (bool Success, string Message) Register(string fullName, string email, string password, string confirmPassword)
        {
            // Validate input
            if (!ValidationHelper.IsValidName(fullName))
                return (false, "Name must be between 2 and 100 characters.");

            if (!ValidationHelper.IsValidEmail(email))
                return (false, "Please enter a valid email address.");

            if (!ValidationHelper.IsValidPassword(password))
                return (false, "Password must be at least 6 characters with uppercase, lowercase, and a digit.");

            if (password != confirmPassword)
                return (false, "Passwords do not match.");

            // Check if email exists
            var existing = _userRepo.GetByEmail(email);
            if (existing != null)
                return (false, "An account with this email already exists.");

            // Create user
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(password),
                Role = "User"
            };

            try
            {
                int userId = _userRepo.Create(user);
                _activityLogRepo.Create(userId, "Register", $"User {fullName} registered");
                return (true, "Registration successful! You can now log in.");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempts to log in a user. Returns (success, errorMessage, user).
        /// </summary>
        public (bool Success, string Message, User? User) Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter both email and password.", null);

            var user = _userRepo.GetByEmail(email);
            if (user == null)
                return (false, "Invalid email or password.", null);

            // Check if account is blocked
            if (user.IsBlocked)
                return (false, "Your account has been blocked. Contact an administrator.", null);

            // Verify password
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                // Increment failed attempts
                user.FailedAttempts++;
                _userRepo.UpdateFailedAttempts(user.UserID, user.FailedAttempts);
                _loginHistoryRepo.Create(user.UserID, "Failed");

                if (user.FailedAttempts >= MaxFailedAttempts)
                {
                    _userRepo.SetBlocked(user.UserID, true);
                    _activityLogRepo.Create(user.UserID, "AccountLocked",
                        $"Account locked after {MaxFailedAttempts} failed attempts");
                    return (false, "Account locked due to too many failed attempts. Contact an administrator.", null);
                }

                int remaining = MaxFailedAttempts - user.FailedAttempts;
                return (false, $"Invalid email or password. {remaining} attempt(s) remaining.", null);
            }

            // Successful login
            _userRepo.UpdateFailedAttempts(user.UserID, 0);
            _userRepo.UpdateLastLogin(user.UserID);
            _loginHistoryRepo.Create(user.UserID, "Success");
            _activityLogRepo.Create(user.UserID, "Login", "User logged in successfully");

            // Update session
            user.FailedAttempts = 0;
            user.LastLogin = DateTime.Now;
            SessionManager.CurrentUser = user;

            return (true, "Login successful!", user);
        }

        /// <summary>
        /// Changes the user's password. Returns (success, errorMessage).
        /// </summary>
        public (bool Success, string Message) ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword)
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
                return (false, "User not found.");

            if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
                return (false, "Current password is incorrect.");

            if (!ValidationHelper.IsValidPassword(newPassword))
                return (false, "New password must be at least 6 characters with uppercase, lowercase, and a digit.");

            if (newPassword != confirmPassword)
                return (false, "New passwords do not match.");

            if (currentPassword == newPassword)
                return (false, "New password must be different from current password.");

            try
            {
                string newHash = PasswordHelper.HashPassword(newPassword);
                _userRepo.UpdatePassword(userId, newHash);
                _activityLogRepo.Create(userId, "ChangePassword", "Password changed successfully");
                return (true, "Password changed successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to change password: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs the user out and clears the session.
        /// </summary>
        public void Logout()
        {
            if (SessionManager.CurrentUser != null)
            {
                _activityLogRepo.Create(SessionManager.CurrentUserID, "Logout", "User logged out");
            }
            SessionManager.Logout();
        }
    }
}
