// ============================================
// SecureVault - File Storage Service
// Physical file management on local disk
// ============================================

using System.Configuration;

namespace SecureVault.BLL
{
    /// <summary>
    /// Manages physical file storage in the local file system.
    /// Files are stored in: SecureVaultStorage/User_{UserID}/
    /// </summary>
    public class FileStorageService
    {
        private readonly string _basePath;

        public FileStorageService()
        {
            string configPath = ConfigurationManager.AppSettings["StorageBasePath"] ?? "SecureVaultStorage";
            _basePath = Path.IsPathRooted(configPath)
                ? configPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath);
        }

        /// <summary>
        /// Gets the storage directory for a specific user.
        /// </summary>
        public string GetUserStoragePath(int userId)
        {
            string path = Path.Combine(_basePath, $"User_{userId}");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Saves a file to the user's storage directory.
        /// Returns the saved file path.
        /// </summary>
        public async Task<string> SaveFileAsync(int userId, string sourceFilePath, string fileName)
        {
            string userDir = GetUserStoragePath(userId);
            string safeName = Helpers.ValidationHelper.SanitizeFileName(fileName);

            // Handle duplicate file names by appending a number
            string destPath = Path.Combine(userDir, safeName);
            int counter = 1;
            string nameWithoutExt = Path.GetFileNameWithoutExtension(safeName);
            string ext = Path.GetExtension(safeName);

            while (File.Exists(destPath))
            {
                destPath = Path.Combine(userDir, $"{nameWithoutExt}_{counter}{ext}");
                counter++;
            }

            // Copy file asynchronously
            using var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destStream);

            return destPath;
        }

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        public bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the total storage size used by a user (from file system).
        /// </summary>
        public long GetUserStorageSize(int userId)
        {
            string userDir = GetUserStoragePath(userId);
            if (!Directory.Exists(userDir)) return 0;

            return new DirectoryInfo(userDir)
                .GetFiles("*", SearchOption.AllDirectories)
                .Sum(f => f.Length);
        }

        /// <summary>
        /// Checks if a file exists in storage.
        /// </summary>
        public bool FileExists(string filePath) => File.Exists(filePath);

        /// <summary>
        /// Gets the base storage path.
        /// </summary>
        public string GetBasePath() => _basePath;
    }
}
