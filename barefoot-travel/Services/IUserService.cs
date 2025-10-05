using barefoot_travel.DTOs.Auth;

namespace barefoot_travel.Services
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<List<UserProfileDto>> GetAllUsersAsync();
    }
}
