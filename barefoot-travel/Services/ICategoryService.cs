using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;

namespace barefoot_travel.Services
{
    public interface ICategoryService
    {
        // Category CRUD operations
        Task<ApiResponse> GetCategoryByIdAsync(int id);
        Task<ApiResponse> GetAllCategoriesAsync();
        Task<PagedResult<CategoryDto>> GetCategoriesPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? type = null, bool? active = null);
        Task<ApiResponse> CreateCategoryAsync(CreateCategoryDto dto, string adminUsername);
        Task<ApiResponse> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string adminUsername);
        Task<ApiResponse> DeleteCategoryAsync(int id, string adminUsername);
        Task<ApiResponse> UpdateCategoryStatusAsync(int id, bool active, string adminUsername);
        Task<ApiResponse> GetCategoriesByTypeAsync(string type);
        Task<ApiResponse> GetCategoriesByParentIdAsync(int? parentId);
        Task<ApiResponse> GetAllType();
        Task<ApiResponse> GetCategoryTreeAsync();
    }
}
