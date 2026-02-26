// ============================================
// SecureVault - User Repository
// Data access for Users table
// ============================================

using Microsoft.Data.SqlClient;
using SecureVault.Models;

namespace SecureVault.DAL
{
    /// <summary>
    /// Repository for Users table operations.
    /// All queries are parameterized for security.
    /// </summary>
    public class UserRepository
    {
        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        public User? GetByEmail(string email)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT * FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapUser(reader);
            return null;
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        public User? GetById(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT * FROM Users WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapUser(reader);
            return null;
        }

        /// <summary>
        /// Gets all users, optionally filtered by search term.
        /// </summary>
        public List<User> GetAll(string? searchTerm = null)
        {
            var users = new List<User>();
            using var conn = DatabaseHelper.GetConnection();

            string sql = "SELECT * FROM Users";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                sql += " WHERE FullName LIKE @Search OR Email LIKE @Search";
            sql += " ORDER BY CreatedAt DESC";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                users.Add(MapUser(reader));

            return users;
        }

        /// <summary>
        /// Creates a new user. Returns the new UserID.
        /// </summary>
        public int Create(User user)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Users (FullName, Email, PasswordHash, Role, ProfileImagePath, IsBlocked, FailedAttempts, CreatedAt)
                VALUES (@FullName, @Email, @PasswordHash, @Role, @ProfileImagePath, @IsBlocked, @FailedAttempts, GETDATE());
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@ProfileImagePath", (object?)user.ProfileImagePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
            cmd.Parameters.AddWithValue("@FailedAttempts", user.FailedAttempts);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Updates user profile information (Name, Email, ProfileImagePath).
        /// </summary>
        public void UpdateProfile(User user)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE Users SET FullName = @FullName, Email = @Email, 
                ProfileImagePath = @ProfileImagePath
                WHERE UserID = @UserID", conn);

            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@ProfileImagePath", (object?)user.ProfileImagePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserID", user.UserID);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the user's password hash.
        /// </summary>
        public void UpdatePassword(int userId, string newPasswordHash)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Users SET PasswordHash = @Hash WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@Hash", newPasswordHash);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the failed login attempt count.
        /// </summary>
        public void UpdateFailedAttempts(int userId, int attempts)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Users SET FailedAttempts = @Attempts WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@Attempts", attempts);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Blocks or unblocks a user account.
        /// </summary>
        public void SetBlocked(int userId, bool isBlocked)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Users SET IsBlocked = @IsBlocked, FailedAttempts = 0 WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@IsBlocked", isBlocked);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates the last login timestamp.
        /// </summary>
        public void UpdateLastLogin(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a user account permanently.
        /// </summary>
        public void Delete(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "DELETE FROM Users WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets total count of users (excluding admins).
        /// </summary>
        public int GetTotalUserCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Role = 'User'", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets the most active user (by document count).
        /// </summary>
        public User? GetMostActiveUser()
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 u.* FROM Users u
                INNER JOIN Documents d ON u.UserID = d.UserID
                WHERE d.IsDeleted = 0
                GROUP BY u.UserID, u.FullName, u.Email, u.PasswordHash, u.Role, 
                         u.ProfileImagePath, u.IsBlocked, u.FailedAttempts, u.LastLogin, u.CreatedAt
                ORDER BY COUNT(d.DocumentID) DESC", conn);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapUser(reader);
            return null;
        }

        /// <summary>
        /// Maps a data reader row to a User object.
        /// </summary>
        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                ProfileImagePath = reader.IsDBNull(reader.GetOrdinal("ProfileImagePath"))
                    ? null : reader.GetString(reader.GetOrdinal("ProfileImagePath")),
                IsBlocked = reader.GetBoolean(reader.GetOrdinal("IsBlocked")),
                FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
                LastLogin = reader.IsDBNull(reader.GetOrdinal("LastLogin"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("LastLogin")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
