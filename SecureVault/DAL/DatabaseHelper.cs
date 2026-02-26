// ============================================
// SecureVault - Database Helper
// Centralized database connection management
// ============================================

using System.Configuration;
using Microsoft.Data.SqlClient;

namespace SecureVault.DAL
{
    /// <summary>
    /// Provides centralized database connection management.
    /// All database access goes through this helper.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string _connectionString;

        static DatabaseHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SecureVaultDB"]?.ConnectionString
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=SecureVaultDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        /// <summary>
        /// Gets a new, opened SQL connection.
        /// Caller is responsible for disposing.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Tests the database connection. Returns true if successful.
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using var conn = GetConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
