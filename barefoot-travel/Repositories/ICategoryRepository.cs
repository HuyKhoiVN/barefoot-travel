using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface ICategoryRepository
    {
        // Category CRUD operations
        Task<Category?> GetByIdAsync(int id);
        Task<List<Category>> GetAllAsync();
        Task<PagedResult<Category>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string categoryName, int? excludeId = null);
        Task<bool> HasChildrenAsync(int id);
        Task<Category?> GetBySlugAsync(string slug);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
        Task<List<string>> GetAllSlugsAsync();
        Task<List<Category>> GetByTypeAsync(string type);
        Task<List<Category>> GetByParentIdAsync(int? parentId);
        Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy);
        Task<List<string>> GetAllType();
        Task<List<int>> GetDescendantCategoryIds(int parentId);
        Task<PagedResult<Category>> GetTreePagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null);
        Task<List<Category>> GetChildrenAsync(int parentId);
        
        // Homepage configuration methods
        Task<List<Category>> GetCategoriesWithHomepageConfigAsync();
        Task<Category?> GetCategoryWithHomepageByIdAsync(int id);
        
        // Daily Tours methods
        Task<List<Category>> GetDailyTourCategoriesAsync();
        Task<Category?> GetDailyTourCategoryByIdAsync(int id);
        Task<int> GetMaxDailyTourOrderAsync();
    }
}
