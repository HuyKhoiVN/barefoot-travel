using barefoot_travel.Common;
using barefoot_travel.DTOs.Role;

namespace barefoot_travel.Services
{
    public interface IRoleService
    {
        Task<ApiResponse> GetRoleByIdAsync(int id);
        Task<ApiResponse> GetAllRolesAsync();
        Task<ApiResponse> CreateRoleAsync(CreateRoleDto dto);
        Task<ApiResponse> UpdateRoleAsync(int id, UpdateRoleDto dto);
        Task<ApiResponse> DeleteRoleAsync(int id);
    }
}
