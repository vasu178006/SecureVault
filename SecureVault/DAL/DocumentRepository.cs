// ============================================
// SecureVault - Document Repository
// Data access for Documents table
// ============================================

using Microsoft.Data.SqlClient;
using SecureVault.Models;

namespace SecureVault.DAL
{
    /// <summary>
    /// Repository for Documents table operations.
    /// All queries are parameterized for security.
    /// </summary>
    public class DocumentRepository
    {
        /// <summary>
        /// Gets documents for a specific user (not deleted).
        /// </summary>
        public List<Document> GetByUserId(int userId, string? searchTerm = null, int? categoryId = null,
            string? fileType = null, DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, bool sortAsc = true, int page = 1, int pageSize = 50)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();

            var sql = @"SELECT d.*, c.CategoryName FROM Documents d
                        LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                        WHERE d.UserID = @UserID AND d.IsDeleted = 0";

            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.Parameters.AddWithValue("@UserID", userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (d.FileName LIKE @Search OR d.Tags LIKE @Search OR d.Description LIKE @Search)";
                cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
            }
            if (categoryId.HasValue)
            {
                sql += " AND d.CategoryID = @CategoryID";
                cmd.Parameters.AddWithValue("@CategoryID", categoryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(fileType))
            {
                sql += " AND d.FileType = @FileType";
                cmd.Parameters.AddWithValue("@FileType", fileType);
            }
            if (dateFrom.HasValue)
            {
                sql += " AND d.UploadDate >= @DateFrom";
                cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                sql += " AND d.UploadDate <= @DateTo";
                cmd.Parameters.AddWithValue("@DateTo", dateTo.Value.AddDays(1));
            }

            // Sorting
            sql += sortBy?.ToLower() switch
            {
                "name" => sortAsc ? " ORDER BY d.FileName ASC" : " ORDER BY d.FileName DESC",
                "size" => sortAsc ? " ORDER BY d.FileSize ASC" : " ORDER BY d.FileSize DESC",
                "type" => sortAsc ? " ORDER BY d.FileType ASC" : " ORDER BY d.FileType DESC",
                _ => " ORDER BY d.UploadDate DESC"
            };

            // Pagination
            sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Gets all documents across all users (for admin). 
        /// </summary>
        public List<Document> GetAll(string? searchTerm = null, int page = 1, int pageSize = 50)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();

            var sql = @"SELECT d.*, c.CategoryName FROM Documents d
                        LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                        WHERE d.IsDeleted = 0";

            var cmd = new SqlCommand();
            cmd.Connection = conn;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (d.FileName LIKE @Search OR d.Tags LIKE @Search)";
                cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
            }

            sql += " ORDER BY d.UploadDate DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Gets a document by its ID.
        /// </summary>
        public Document? GetById(int documentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.DocumentID = @DocumentID", conn);
            cmd.Parameters.AddWithValue("@DocumentID", documentId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapDocument(reader);
            return null;
        }

        /// <summary>
        /// Creates a new document record. Returns the new DocumentID.
        /// </summary>
        public int Create(Document doc)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Documents (UserID, FileName, FilePath, FileType, FileSize, CategoryID, 
                    Description, Tags, IsImportant, IsDeleted, UploadDate)
                VALUES (@UserID, @FileName, @FilePath, @FileType, @FileSize, @CategoryID,
                    @Description, @Tags, @IsImportant, 0, GETDATE());
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.AddWithValue("@UserID", doc.UserID);
            cmd.Parameters.AddWithValue("@FileName", doc.FileName);
            cmd.Parameters.AddWithValue("@FilePath", doc.FilePath);
            cmd.Parameters.AddWithValue("@FileType", doc.FileType);
            cmd.Parameters.AddWithValue("@FileSize", doc.FileSize);
            cmd.Parameters.AddWithValue("@CategoryID", (object?)doc.CategoryID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Description", (object?)doc.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Tags", (object?)doc.Tags ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsImportant", doc.IsImportant);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Updates document metadata (name, description, tags, category, importance).
        /// </summary>
        public void Update(Document doc)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE Documents SET FileName = @FileName, Description = @Description,
                    Tags = @Tags, CategoryID = @CategoryID, IsImportant = @IsImportant
                WHERE DocumentID = @DocumentID", conn);

            cmd.Parameters.AddWithValue("@FileName", doc.FileName);
            cmd.Parameters.AddWithValue("@Description", (object?)doc.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Tags", (object?)doc.Tags ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryID", (object?)doc.CategoryID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsImportant", doc.IsImportant);
            cmd.Parameters.AddWithValue("@DocumentID", doc.DocumentID);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Soft deletes a document (sets IsDeleted = 1).
        /// </summary>
        public void SoftDelete(int documentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Documents SET IsDeleted = 1 WHERE DocumentID = @DocumentID", conn);
            cmd.Parameters.AddWithValue("@DocumentID", documentId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Restores a soft-deleted document.
        /// </summary>
        public void Restore(int documentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Documents SET IsDeleted = 0 WHERE DocumentID = @DocumentID", conn);
            cmd.Parameters.AddWithValue("@DocumentID", documentId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Permanently deletes a document from the database.
        /// </summary>
        public void PermanentDelete(int documentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "DELETE FROM Documents WHERE DocumentID = @DocumentID", conn);
            cmd.Parameters.AddWithValue("@DocumentID", documentId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets deleted documents for a user (recycle bin).
        /// </summary>
        public List<Document> GetDeleted(int userId)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.UserID = @UserID AND d.IsDeleted = 1
                ORDER BY d.UploadDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Gets recently uploaded documents.
        /// </summary>
        public List<Document> GetRecent(int userId, int count = 5)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP (@Count) d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.UserID = @UserID AND d.IsDeleted = 0
                ORDER BY d.UploadDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Gets important documents for a user.
        /// </summary>
        public List<Document> GetImportant(int userId)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.UserID = @UserID AND d.IsDeleted = 0 AND d.IsImportant = 1
                ORDER BY d.UploadDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Updates the last viewed date for a document.
        /// </summary>
        public void UpdateLastViewed(int documentId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "UPDATE Documents SET LastViewedDate = GETDATE() WHERE DocumentID = @DocumentID", conn);
            cmd.Parameters.AddWithValue("@DocumentID", documentId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets total storage used by a user (in bytes).
        /// </summary>
        public long GetStorageUsed(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT ISNULL(SUM(FileSize), 0) FROM Documents WHERE UserID = @UserID AND IsDeleted = 0", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets total storage used across all users (in bytes).
        /// </summary>
        public long GetTotalStorageUsed()
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT ISNULL(SUM(FileSize), 0) FROM Documents WHERE IsDeleted = 0", conn);
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets total document count for a user.
        /// </summary>
        public int GetDocumentCount(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Documents WHERE UserID = @UserID AND IsDeleted = 0", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets the count of documents uploaded this month for a user.
        /// </summary>
        public int GetThisMonthCount(int userId)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Documents WHERE UserID = @UserID AND IsDeleted = 0 " +
                "AND MONTH(UploadDate) = MONTH(GETDATE()) AND YEAR(UploadDate) = YEAR(GETDATE())", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets total document count across all users.
        /// </summary>
        public int GetTotalDocumentCount()
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Documents WHERE IsDeleted = 0", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// Gets document distribution by category (for charts).
        /// </summary>
        public Dictionary<string, int> GetCategoryDistribution()
        {
            var result = new Dictionary<string, int>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT ISNULL(c.CategoryName, 'Uncategorized') as Cat, COUNT(*) as Cnt 
                FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.IsDeleted = 0
                GROUP BY c.CategoryName
                ORDER BY Cnt DESC", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result[reader.GetString(0)] = reader.GetInt32(1);

            return result;
        }

        /// <summary>
        /// Checks for duplicate files (by name and size).
        /// </summary>
        public List<Document> FindDuplicates(int userId, string fileName, long fileSize)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.UserID = @UserID AND d.IsDeleted = 0 
                AND d.FileName = @FileName AND d.FileSize = @FileSize", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@FileName", fileName);
            cmd.Parameters.AddWithValue("@FileSize", fileSize);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Gets recently viewed documents.
        /// </summary>
        public List<Document> GetRecentlyViewed(int userId, int count = 5)
        {
            var docs = new List<Document>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP (@Count) d.*, c.CategoryName FROM Documents d
                LEFT JOIN Categories c ON d.CategoryID = c.CategoryID
                WHERE d.UserID = @UserID AND d.IsDeleted = 0 AND d.LastViewedDate IS NOT NULL
                ORDER BY d.LastViewedDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                docs.Add(MapDocument(reader));

            return docs;
        }

        /// <summary>
        /// Maps a data reader row to a Document object.
        /// </summary>
        private Document MapDocument(SqlDataReader reader)
        {
            return new Document
            {
                DocumentID = reader.GetInt32(reader.GetOrdinal("DocumentID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                FilePath = reader.GetString(reader.GetOrdinal("FilePath")),
                FileType = reader.GetString(reader.GetOrdinal("FileType")),
                FileSize = reader.GetInt64(reader.GetOrdinal("FileSize")),
                CategoryID = reader.IsDBNull(reader.GetOrdinal("CategoryID"))
                    ? null : reader.GetInt32(reader.GetOrdinal("CategoryID")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
                    ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null : reader.GetString(reader.GetOrdinal("Description")),
                Tags = reader.IsDBNull(reader.GetOrdinal("Tags"))
                    ? null : reader.GetString(reader.GetOrdinal("Tags")),
                IsImportant = reader.GetBoolean(reader.GetOrdinal("IsImportant")),
                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                UploadDate = reader.GetDateTime(reader.GetOrdinal("UploadDate")),
                LastViewedDate = reader.IsDBNull(reader.GetOrdinal("LastViewedDate"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("LastViewedDate"))
            };
        }
    }
}
