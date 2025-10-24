using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Policy;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IPolicyRepository
    {
        // Policy CRUD operations
        Task<Policy?> GetByIdAsync(int id);
        Task<List<Policy>> GetAllAsync();
        Task<PagedResult<Policy>> GetPagedAsync(int page, int pageSize, string? policyType = null, string? sortBy = null, string? sortOrder = "asc", bool? active = null);
        Task<Policy> CreateAsync(Policy policy);
        Task<Policy> UpdateAsync(Policy policy);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TypeExistsAsync(string policyType, int? excludeId = null);
        Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy);
    }
}
