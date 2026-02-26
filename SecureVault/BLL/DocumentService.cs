// ============================================
// SecureVault - Document Service
// Business logic for document management
// ============================================

using SecureVault.DAL;
using SecureVault.Helpers;
using SecureVault.Models;
using System.Configuration;
using System.Text;

namespace SecureVault.BLL
{
    /// <summary>
    /// Handles document upload, management, search, and export.
    /// </summary>
    public class DocumentService
    {
        private readonly DocumentRepository _docRepo = new();
        private readonly ActivityLogRepository _activityLogRepo = new();
        private readonly FileStorageService _fileService = new();

        private int MaxFileSizeMB
        {
            get
            {
                string? val = ConfigurationManager.AppSettings["MaxFileSizeMB"];
                return int.TryParse(val, out int max) ? max : 50;
            }
        }

        private string AllowedFileTypes =>
            ConfigurationManager.AppSettings["AllowedFileTypes"] ?? ".pdf,.jpg,.jpeg,.png,.docx,.doc,.xlsx,.txt";

        /// <summary>
        /// Uploads a file. Returns (success, message, documentId).
        /// </summary>
        public async Task<(bool Success, string Message, int DocumentId)> UploadAsync(
            int userId, string sourceFilePath, int? categoryId = null,
            string? description = null, string? tags = null)
        {
            string fileName = Path.GetFileName(sourceFilePath);
            string fileType = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            long fileSize = new FileInfo(sourceFilePath).Length;

            // Validate file type
            if (!ValidationHelper.IsAllowedFileType(fileName, AllowedFileTypes))
                return (false, $"File type '{fileType}' is not allowed. Allowed types: {AllowedFileTypes}", 0);

            // Validate file size
            if (!ValidationHelper.IsWithinSizeLimit(fileSize, MaxFileSizeMB))
                return (false, $"File exceeds the maximum size limit of {MaxFileSizeMB} MB.", 0);

            try
            {
                // Check for duplicates
                var duplicates = _docRepo.FindDuplicates(userId, fileName, fileSize);
                if (duplicates.Count > 0)
                {
                    // Allow upload but warn; we'll append a number to the filename
                }

                // Save file to storage
                string savedPath = await _fileService.SaveFileAsync(userId, sourceFilePath, fileName);
                string savedFileName = Path.GetFileName(savedPath);

                // Create database record
                var doc = new Document
                {
                    UserID = userId,
                    FileName = savedFileName,
                    FilePath = savedPath,
                    FileType = fileType,
                    FileSize = fileSize,
                    CategoryID = categoryId,
                    Description = description,
                    Tags = tags
                };

                int docId = _docRepo.Create(doc);
                _activityLogRepo.Create(userId, "Upload", $"Uploaded file: {savedFileName}");

                string msg = duplicates.Count > 0
                    ? $"File uploaded successfully (duplicate detected, saved as {savedFileName})."
                    : "File uploaded successfully!";

                return (true, msg, docId);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Gets documents for a user with search/filter support.
        /// </summary>
        public List<Document> GetDocuments(int userId, string? searchTerm = null, int? categoryId = null,
            string? fileType = null, DateTime? dateFrom = null, DateTime? dateTo = null,
            string? sortBy = null, bool sortAsc = true, int page = 1, int pageSize = 50)
        {
            return _docRepo.GetByUserId(userId, searchTerm, categoryId, fileType, dateFrom, dateTo,
                sortBy, sortAsc, page, pageSize);
        }

        /// <summary>
        /// Gets all documents (admin only).
        /// </summary>
        public List<Document> GetAllDocuments(string? searchTerm = null, int page = 1, int pageSize = 50)
        {
            return _docRepo.GetAll(searchTerm, page, pageSize);
        }

        /// <summary>
        /// Gets a document by ID.
        /// </summary>
        public Document? GetById(int documentId)
        {
            return _docRepo.GetById(documentId);
        }

        /// <summary>
        /// Updates document metadata.
        /// </summary>
        public (bool Success, string Message) UpdateDocument(Document doc)
        {
            try
            {
                _docRepo.Update(doc);
                _activityLogRepo.Create(doc.UserID, "EditDocument", $"Edited document: {doc.FileName}");
                return (true, "Document updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update document: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft deletes a document.
        /// </summary>
        public (bool Success, string Message) SoftDelete(int documentId, int userId)
        {
            var doc = _docRepo.GetById(documentId);
            if (doc == null) return (false, "Document not found.");

            try
            {
                _docRepo.SoftDelete(documentId);
                _activityLogRepo.Create(userId, "Delete", $"Deleted document: {doc.FileName}");
                return (true, "Document moved to recycle bin.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete document: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores a soft-deleted document.
        /// </summary>
        public (bool Success, string Message) Restore(int documentId, int userId)
        {
            var doc = _docRepo.GetById(documentId);
            if (doc == null) return (false, "Document not found.");

            try
            {
                _docRepo.Restore(documentId);
                _activityLogRepo.Create(userId, "Restore", $"Restored document: {doc.FileName}");
                return (true, "Document restored successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to restore document: {ex.Message}");
            }
        }

        /// <summary>
        /// Permanently deletes a document (admin only).
        /// </summary>
        public (bool Success, string Message) PermanentDelete(int documentId, int userId)
        {
            var doc = _docRepo.GetById(documentId);
            if (doc == null) return (false, "Document not found.");

            try
            {
                _fileService.DeleteFile(doc.FilePath);
                _docRepo.PermanentDelete(documentId);
                _activityLogRepo.Create(userId, "PermanentDelete", $"Permanently deleted: {doc.FileName}");
                return (true, "Document permanently deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to permanently delete document: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles the IsImportant flag on a document.
        /// </summary>
        public void ToggleImportant(int documentId)
        {
            var doc = _docRepo.GetById(documentId);
            if (doc != null)
            {
                doc.IsImportant = !doc.IsImportant;
                _docRepo.Update(doc);
            }
        }

        /// <summary>
        /// Updates the last viewed date for a document.
        /// </summary>
        public void MarkAsViewed(int documentId) => _docRepo.UpdateLastViewed(documentId);

        // Dashboard & statistics methods
        public List<Document> GetRecent(int userId, int count = 5) => _docRepo.GetRecent(userId, count);
        public List<Document> GetImportant(int userId) => _docRepo.GetImportant(userId);
        public List<Document> GetDeleted(int userId) => _docRepo.GetDeleted(userId);
        public List<Document> GetRecentlyViewed(int userId, int count = 5) => _docRepo.GetRecentlyViewed(userId, count);
        public int GetDocumentCount(int userId) => _docRepo.GetDocumentCount(userId);
        public int GetThisMonthCount(int userId) => _docRepo.GetThisMonthCount(userId);
        public int GetTotalDocumentCount() => _docRepo.GetTotalDocumentCount();
        public long GetStorageUsed(int userId) => _docRepo.GetStorageUsed(userId);
        public long GetTotalStorageUsed() => _docRepo.GetTotalStorageUsed();
        public Dictionary<string, int> GetCategoryDistribution() => _docRepo.GetCategoryDistribution();
        public List<Document> FindDuplicates(int userId, string fileName, long fileSize)
            => _docRepo.FindDuplicates(userId, fileName, fileSize);

        /// <summary>
        /// Exports document list to CSV.
        /// </summary>
        public string ExportToCsv(List<Document> documents)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DocumentID,FileName,FileType,FileSize,Category,Tags,UploadDate,IsImportant");

            foreach (var doc in documents)
            {
                sb.AppendLine($"{doc.DocumentID},\"{doc.FileName}\",{doc.FileType},{doc.FileSize}," +
                    $"\"{doc.CategoryName ?? ""}\",\"{doc.Tags ?? ""}\",{doc.UploadDate:yyyy-MM-dd HH:mm},{doc.IsImportant}");
            }

            return sb.ToString();
        }
    }
}
