// ============================================
// SecureVault - ActivityLog Repository
// Data access for ActivityLogs table
// ============================================

using Microsoft.Data.SqlClient;
using SecureVault.Models;

namespace SecureVault.DAL
{
    /// <summary>
    /// Repository for ActivityLogs table operations.
    /// </summary>
    public class ActivityLogRepository
    {
        /// <summary>
        /// Logs a new activity.
        /// </summary>
        public void Create(int userId, string action, string? description = null)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO ActivityLogs (UserID, Action, Description, Timestamp)
                VALUES (@UserID, @Action, @Description, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets activity logs for a specific user.
        /// </summary>
        public List<ActivityLog> GetByUserId(int userId, DateTime? dateFrom = null, DateTime? dateTo = null,
            int page = 1, int pageSize = 50)
        {
            var logs = new List<ActivityLog>();
            using var conn = DatabaseHelper.GetConnection();

            var sql = @"SELECT a.*, u.FullName as UserName FROM ActivityLogs a
                        INNER JOIN Users u ON a.UserID = u.UserID
                        WHERE a.UserID = @UserID";
            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@UserID", userId);

            if (dateFrom.HasValue)
            {
                sql += " AND a.Timestamp >= @DateFrom";
                cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                sql += " AND a.Timestamp <= @DateTo";
                cmd.Parameters.AddWithValue("@DateTo", dateTo.Value.AddDays(1));
            }

            sql += " ORDER BY a.Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(MapLog(reader));

            return logs;
        }

        /// <summary>
        /// Gets all activity logs (for admin).
        /// </summary>
        public List<ActivityLog> GetAll(int? filterUserId = null, DateTime? dateFrom = null,
            DateTime? dateTo = null, int page = 1, int pageSize = 50)
        {
            var logs = new List<ActivityLog>();
            using var conn = DatabaseHelper.GetConnection();

            var sql = @"SELECT a.*, u.FullName as UserName FROM ActivityLogs a
                        INNER JOIN Users u ON a.UserID = u.UserID WHERE 1=1";
            var cmd = new SqlCommand();
            cmd.Connection = conn;

            if (filterUserId.HasValue)
            {
                sql += " AND a.UserID = @FilterUserID";
                cmd.Parameters.AddWithValue("@FilterUserID", filterUserId.Value);
            }
            if (dateFrom.HasValue)
            {
                sql += " AND a.Timestamp >= @DateFrom";
                cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                sql += " AND a.Timestamp <= @DateTo";
                cmd.Parameters.AddWithValue("@DateTo", dateTo.Value.AddDays(1));
            }

            sql += " ORDER BY a.Timestamp DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                logs.Add(MapLog(reader));

            return logs;
        }

        private ActivityLog MapLog(SqlDataReader reader)
        {
            return new ActivityLog
            {
                LogID = reader.GetInt32(reader.GetOrdinal("LogID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.IsDBNull(reader.GetOrdinal("UserName"))
                    ? null : reader.GetString(reader.GetOrdinal("UserName")),
                Action = reader.GetString(reader.GetOrdinal("Action")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null : reader.GetString(reader.GetOrdinal("Description")),
                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
            };
        }
    }
}
