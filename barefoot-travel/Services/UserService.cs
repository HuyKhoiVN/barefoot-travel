using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Auth;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Services
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IAccountRepository accountRepository, ILogger<UserService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        #region Existing Methods

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

        #endregion

        #region CRUD Operations

        public async Task<ApiResponse> GetUserByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting user with ID: {UserId}", id);

                if (id <= 0)
                {
                    return new ApiResponse(false, "Invalid user ID");
                }

                var user = await _accountRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new ApiResponse(false, "User not found");
                }

                var roleName = GetRoleName(user.RoleId);
                var userDto = MapToUserProfileDto(user, new List<string> { roleName });

                return new ApiResponse(true, "User retrieved successfully", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<PagedResult<UserProfileDto>> GetUsersPagedAsync(int page, int pageSize, string? sortBy = null, string sortOrder = "asc", string? search = null, int? roleId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                _logger.LogInformation("Getting paged users - Page: {Page}, PageSize: {PageSize}, Search: {Search}, RoleId: {RoleId}", page, pageSize, search, roleId);

                var users = await _accountRepository.GetPagedAsync(page, pageSize, sortBy, sortOrder, search, roleId, dateFrom, dateTo);
                
                var userDtos = users.Items.Select(user =>
                {
                    var roleName = GetRoleName(user.RoleId);
                    return MapToUserProfileDto(user, new List<string> { roleName });
                }).ToList();

                return new PagedResult<UserProfileDto>
                {
                    Items = userDtos,
                    TotalItems = users.TotalItems,
                    TotalPages = users.TotalPages,
                    CurrentPage = users.CurrentPage,
                    PageSize = users.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged users");
                throw;
            }
        }

        public async Task<ApiResponse> CreateUserAsync(CreateUserDto dto, string createdBy)
        {
            try
            {
                _logger.LogInformation("Creating user: {Username}", dto.Username);

                // Check if username already exists
                var existingUser = await _accountRepository.GetByUsernameAsync(dto.Username);
                if (existingUser != null)
                {
                    return new ApiResponse(false, "Username already exists");
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = MapToAccount(dto, passwordHash, createdBy);
                var createdUser = await _accountRepository.CreateAsync(user);

                var roleName = GetRoleName(createdUser.RoleId);
                var userDto = MapToUserProfileDto(createdUser, new List<string> { roleName });

                return new ApiResponse(true, "User created successfully", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", dto.Username);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateUserAsync(int id, UpdateUserDto dto, string updatedBy)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", id);

                var existingUser = await _accountRepository.GetByIdAsync(id);
                if (existingUser == null)
                {
                    return new ApiResponse(false, "User not found");
                }

                MapToAccountForUpdate(existingUser, dto, updatedBy);
                var updatedUser = await _accountRepository.UpdateAsync(existingUser);

                var roleName = GetRoleName(updatedUser.RoleId);
                var userDto = MapToUserProfileDto(updatedUser, new List<string> { roleName });

                return new ApiResponse(true, "User updated successfully", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(int id, string deletedBy)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", id);

                var user = await _accountRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new ApiResponse(false, "User not found");
                }

                // Soft delete - set Active to false
                user.Active = false;
                user.UpdatedTime = DateTime.UtcNow;
                user.UpdatedBy = deletedBy;

                await _accountRepository.UpdateAsync(user);

                return new ApiResponse(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        #endregion

        #region Additional Operations

        public async Task<ApiResponse> ChangePasswordAsync(int id, ChangePasswordDto dto, string updatedBy)
        {
            try
            {
                _logger.LogInformation("Changing password for user ID: {UserId}", id);

                var user = await _accountRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new ApiResponse(false, "User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return new ApiResponse(false, "Current password is incorrect");
                }

                // Hash new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedTime = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                await _accountRepository.UpdateAsync(user);

                return new ApiResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(int id, PasswordResetDto dto, string updatedBy)
        {
            try
            {
                _logger.LogInformation("Resetting password for user ID: {UserId}", id);

                var user = await _accountRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return new ApiResponse(false, "User not found");
                }

                // Validate password confirmation
                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return new ApiResponse(false, "New password and confirm password do not match");
                }

                // Hash new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedTime = DateTime.UtcNow;
                user.UpdatedBy = updatedBy;

                await _accountRepository.UpdateAsync(user);

                return new ApiResponse(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user ID: {UserId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

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
                Roles = roles,
                CreatedTime = user.CreatedTime,
                UpdatedTime = user.UpdatedTime,
                UpdatedBy = user.UpdatedBy
            };
        }

        private Account MapToAccount(CreateUserDto dto, string passwordHash, string createdBy)
        {
            if (dto == null) return null;

            return new Account
            {
                Username = dto.Username,
                FullName = dto.FullName,
                PasswordHash = passwordHash,
                Email = dto.Email,
                Phone = dto.Phone,
                Photo = dto.Photo,
                RoleId = dto.RoleId,
                Active = true,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = createdBy
            };
        }

        private void MapToAccountForUpdate(Account existingUser, UpdateUserDto dto, string updatedBy)
        {
            if (existingUser == null || dto == null) return;

            existingUser.FullName = dto.FullName;
            existingUser.Email = dto.Email;
            existingUser.Phone = dto.Phone;
            existingUser.Photo = dto.Photo;
            existingUser.RoleId = dto.RoleId;
            existingUser.Active = dto.Active;
            existingUser.UpdatedTime = DateTime.UtcNow;
            existingUser.UpdatedBy = updatedBy;
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

        #endregion
    }
}
