// ============================================
// SecureVault - LoginHistory Repository
// Data access for LoginHistory table
// ============================================

using Microsoft.Data.SqlClient;
using SecureVault.Models;

namespace SecureVault.DAL
{
    /// <summary>
    /// Repository for LoginHistory table operations.
    /// </summary>
    public class LoginHistoryRepository
    {
        /// <summary>
        /// Records a login attempt.
        /// </summary>
        public void Create(int userId, string status)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO LoginHistory (UserID, LoginTime, Status)
                VALUES (@UserID, GETDATE(), @Status)", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets login history for a specific user.
        /// </summary>
        public List<LoginHistory> GetByUserId(int userId, int count = 50)
        {
            var history = new List<LoginHistory>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP (@Count) lh.*, u.FullName as UserName FROM LoginHistory lh
                INNER JOIN Users u ON lh.UserID = u.UserID
                WHERE lh.UserID = @UserID
                ORDER BY lh.LoginTime DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new LoginHistory
                {
                    LoginID = reader.GetInt32(reader.GetOrdinal("LoginID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    UserName = reader.IsDBNull(reader.GetOrdinal("UserName"))
                        ? null : reader.GetString(reader.GetOrdinal("UserName")),
                    LoginTime = reader.GetDateTime(reader.GetOrdinal("LoginTime")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                });
            }

            return history;
        }

        /// <summary>
        /// Gets all login history (for admin).
        /// </summary>
        public List<LoginHistory> GetAll(int count = 100)
        {
            var history = new List<LoginHistory>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP (@Count) lh.*, u.FullName as UserName FROM LoginHistory lh
                INNER JOIN Users u ON lh.UserID = u.UserID
                ORDER BY lh.LoginTime DESC", conn);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new LoginHistory
                {
                    LoginID = reader.GetInt32(reader.GetOrdinal("LoginID")),
                    UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                    UserName = reader.IsDBNull(reader.GetOrdinal("UserName"))
                        ? null : reader.GetString(reader.GetOrdinal("UserName")),
                    LoginTime = reader.GetDateTime(reader.GetOrdinal("LoginTime")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                });
            }

            return history;
        }
    }
}
