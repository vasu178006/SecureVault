// ============================================
// SecureVault - Category Repository
// Data access for Categories table
// ============================================

using Microsoft.Data.SqlClient;
using SecureVault.Models;

namespace SecureVault.DAL
{
    /// <summary>
    /// Repository for Categories table operations.
    /// </summary>
    public class CategoryRepository
    {
        /// <summary>
        /// Gets all categories available to a user (system + user's own).
        /// </summary>
        public List<Category> GetByUserId(int userId)
        {
            var categories = new List<Category>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT c.*, 
                    (SELECT COUNT(*) FROM Documents d WHERE d.CategoryID = c.CategoryID AND d.IsDeleted = 0) as DocumentCount
                FROM Categories c
                WHERE c.UserID IS NULL OR c.UserID = @UserID
                ORDER BY c.CategoryName", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                    UserID = reader.IsDBNull(reader.GetOrdinal("UserID"))
                        ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    DocumentCount = reader.GetInt32(reader.GetOrdinal("DocumentCount"))
                });
            }

            return categories;
        }

        /// <summary>
        /// Gets all categories (for admin).
        /// </summary>
        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT c.*, 
                    (SELECT COUNT(*) FROM Documents d WHERE d.CategoryID = c.CategoryID AND d.IsDeleted = 0) as DocumentCount
                FROM Categories c
                ORDER BY c.CategoryName", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(new Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                    UserID = reader.IsDBNull(reader.GetOrdinal("UserID"))
                        ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    DocumentCount = reader.GetInt32(reader.GetOrdinal("DocumentCount"))
                });
            }

            return categories;
        }

        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        public Category? GetById(int categoryId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT *, 0 as DocumentCount FROM Categories WHERE CategoryID = @CategoryID", conn);
            cmd.Parameters.AddWithValue("@CategoryID", categoryId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Category
                {
                    CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                    UserID = reader.IsDBNull(reader.GetOrdinal("UserID"))
                        ? null : reader.GetInt32(reader.GetOrdinal("UserID")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
                };
            }
            return null;
        }

        /// <summary>
        /// Creates a new category. Returns the new CategoryID.
        /// </summary>
        public int Create(Category category)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Categories (UserID, CategoryName) 
                VALUES (@UserID, @CategoryName);
                SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@UserID", (object?)category.UserID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Deletes a category. Documents in this category will have their CategoryID set to NULL.
        /// </summary>
        public void Delete(int categoryId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "DELETE FROM Categories WHERE CategoryID = @CategoryID", conn);
            cmd.Parameters.AddWithValue("@CategoryID", categoryId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if a category name already exists for a user.
        /// </summary>
        public bool Exists(string categoryName, int? userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Categories 
                WHERE CategoryName = @Name AND (UserID = @UserID OR UserID IS NULL)", conn);
            cmd.Parameters.AddWithValue("@Name", categoryName);
            cmd.Parameters.AddWithValue("@UserID", (object?)userId ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}
