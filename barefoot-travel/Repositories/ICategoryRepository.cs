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
        Task<PagedResult<Category>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? type = null, bool? active = null);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string categoryName, int? excludeId = null);
        Task<bool> HasChildrenAsync(int id);
        Task<List<Category>> GetByTypeAsync(string type);
        Task<List<Category>> GetByParentIdAsync(int? parentId);
        Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy);
        Task<List<string>> GetAllType();
    }
}
