// ============================================
// SecureVault - Category Service
// Business logic for category management
// ============================================

using SecureVault.DAL;
using SecureVault.Models;

namespace SecureVault.BLL
{
    /// <summary>
    /// Handles category creation, listing, and deletion.
    /// </summary>
    public class CategoryService
    {
        private readonly CategoryRepository _catRepo = new();
        private readonly ActivityLogRepository _activityLogRepo = new();

        /// <summary>
        /// Gets all categories available to a user.
        /// </summary>
        public List<Category> GetCategories(int userId) => _catRepo.GetByUserId(userId);

        /// <summary>
        /// Gets all categories (admin).
        /// </summary>
        public List<Category> GetAllCategories() => _catRepo.GetAll();

        /// <summary>
        /// Creates a new custom category.
        /// </summary>
        public (bool Success, string Message) CreateCategory(string categoryName, int userId)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return (false, "Category name cannot be empty.");

            if (categoryName.Length > 100)
                return (false, "Category name is too long (max 100 characters).");

            if (_catRepo.Exists(categoryName, userId))
                return (false, "A category with this name already exists.");

            try
            {
                var category = new Category
                {
                    UserID = userId,
                    CategoryName = categoryName.Trim()
                };
                _catRepo.Create(category);
                _activityLogRepo.Create(userId, "CreateCategory", $"Created category: {categoryName}");
                return (true, "Category created successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create category: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user-created category. System categories cannot be deleted.
        /// </summary>
        public (bool Success, string Message) DeleteCategory(int categoryId, int userId)
        {
            var category = _catRepo.GetById(categoryId);
            if (category == null) return (false, "Category not found.");
            if (category.IsSystemCategory) return (false, "System categories cannot be deleted.");

            try
            {
                _catRepo.Delete(categoryId);
                _activityLogRepo.Create(userId, "DeleteCategory", $"Deleted category: {category.CategoryName}");
                return (true, "Category deleted. Documents in this category are now uncategorized.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete category: {ex.Message}");
            }
        }
    }
}
