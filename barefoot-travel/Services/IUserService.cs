using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Auth;

namespace barefoot_travel.Services
{
    public interface IUserService
    {
        // Existing methods
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<List<UserProfileDto>> GetAllUsersAsync();
        
        // CRUD Operations
        Task<ApiResponse> GetUserByIdAsync(int id);
        Task<PagedResult<UserProfileDto>> GetUsersPagedAsync(int page, int pageSize, string? sortBy = null, string sortOrder = "asc", string? search = null, int? roleId = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<ApiResponse> CreateUserAsync(CreateUserDto dto, string createdBy);
        Task<ApiResponse> UpdateUserAsync(int id, UpdateUserDto dto, string updatedBy);
        Task<ApiResponse> DeleteUserAsync(int id, string deletedBy);
        
        // Additional operations
        Task<ApiResponse> ChangePasswordAsync(int id, ChangePasswordDto dto, string updatedBy);
        Task<ApiResponse> ResetPasswordAsync(int id, PasswordResetDto dto, string updatedBy);
    }
}
