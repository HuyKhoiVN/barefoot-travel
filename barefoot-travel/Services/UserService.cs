using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs.Auth;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepository;

        public UserService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            // Validation in Service layer
            if (userId <= 0)
            {
                throw new BadRequestException("Invalid user ID");
            }

            var user = await _accountRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var roleName = GetRoleName(user.RoleId);
            return MapToUserProfileDto(user, new List<string> { roleName });
        }

        public async Task<List<UserProfileDto>> GetAllUsersAsync()
        {
            var users = await _accountRepository.GetAllAsync();
            var userProfiles = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var roleName = GetRoleName(user.RoleId);
                userProfiles.Add(MapToUserProfileDto(user, new List<string> { roleName }));
            }

            return userProfiles;
        }

        private UserProfileDto MapToUserProfileDto(Account user, List<string> roles)
        {
            if (user == null) return null;

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.Username,
                Name = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = roles
            };
        }

        private string GetRoleName(int roleId)
        {
            // Map RoleId to RoleName based on Database-schema.md
            // Id = 1: Admin, Id = 2: User
            return roleId switch
            {
                1 => "Admin",
                2 => "User",
                _ => "Unknown"
            };
        }
    }
}
