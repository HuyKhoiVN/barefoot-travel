using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Policy;

namespace barefoot_travel.Services
{
    public interface IPolicyService
    {
        // Policy CRUD operations
        Task<ApiResponse> GetPolicyByIdAsync(int id);
        Task<ApiResponse> GetAllPoliciesAsync();
        Task<PagedResult<PolicyDto>> GetPoliciesPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", bool? active = null);
        Task<ApiResponse> CreatePolicyAsync(CreatePolicyDto dto, string adminUsername);
        Task<ApiResponse> UpdatePolicyAsync(int id, UpdatePolicyDto dto, string adminUsername);
        Task<ApiResponse> DeletePolicyAsync(int id, string adminUsername);
        Task<ApiResponse> UpdatePolicyStatusAsync(int id, bool active, string adminUsername);
    }
}
