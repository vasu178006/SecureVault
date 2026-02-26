// ============================================
// SecureVault - Activity Log Service
// Business logic for activity logging
// ============================================

using SecureVault.DAL;
using SecureVault.Models;

namespace SecureVault.BLL
{
    /// <summary>
    /// Handles activity log creation and retrieval.
    /// </summary>
    public class ActivityLogService
    {
        private readonly ActivityLogRepository _logRepo = new();
        private readonly LoginHistoryRepository _loginRepo = new();

        /// <summary>
        /// Logs a new activity.
        /// </summary>
        public void Log(int userId, string action, string? description = null)
        {
            try
            {
                _logRepo.Create(userId, action, description);
            }
            catch
            {
                // Logging should never break the app
            }
        }

        /// <summary>
        /// Gets activity logs for a user.
        /// </summary>
        public List<ActivityLog> GetUserLogs(int userId, DateTime? dateFrom = null, DateTime? dateTo = null,
            int page = 1, int pageSize = 50)
        {
            return _logRepo.GetByUserId(userId, dateFrom, dateTo, page, pageSize);
        }

        /// <summary>
        /// Gets all activity logs (admin).
        /// </summary>
        public List<ActivityLog> GetAllLogs(int? filterUserId = null, DateTime? dateFrom = null,
            DateTime? dateTo = null, int page = 1, int pageSize = 50)
        {
            return _logRepo.GetAll(filterUserId, dateFrom, dateTo, page, pageSize);
        }

        /// <summary>
        /// Gets login history for a user.
        /// </summary>
        public List<LoginHistory> GetLoginHistory(int userId, int count = 50)
        {
            return _loginRepo.GetByUserId(userId, count);
        }

        /// <summary>
        /// Gets all login history (admin).
        /// </summary>
        public List<LoginHistory> GetAllLoginHistory(int count = 100)
        {
            return _loginRepo.GetAll(count);
        }
    }
}
