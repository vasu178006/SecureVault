// ============================================
// SecureVault - Session Manager
// Manages the current logged-in user session
// ============================================

namespace SecureVault.Helpers
{
    /// <summary>
    /// Singleton class to manage current user session state.
    /// </summary>
    public static class SessionManager
    {
        /// <summary>
        /// The currently logged-in user. Null if not logged in.
        /// </summary>
        public static Models.User? CurrentUser { get; set; }

        /// <summary>
        /// Returns true if a user is currently logged in.
        /// </summary>
        public static bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Returns true if the current user is an Admin.
        /// </summary>
        public static bool IsAdmin => CurrentUser?.IsAdmin ?? false;

        /// <summary>
        /// Returns the current user's ID, or -1 if not logged in.
        /// </summary>
        public static int CurrentUserID => CurrentUser?.UserID ?? -1;

        /// <summary>
        /// Clears the session (logout).
        /// </summary>
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
