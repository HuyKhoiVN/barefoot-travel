using barefoot_travel.Common;
using barefoot_travel.DTOs.Auth;

namespace barefoot_travel.Services
{
    public interface IAuthService
    {
        Task<TokenDto> LoginAsync(string username, string password);
        Task<TokenDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<ApiResponse> RegisterAsync(RegisterDto dto);
        Task<UserProfileDto> GetUserProfileAsync(int userId);
    }
}
