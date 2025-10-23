using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Auth;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region User CRUD Operations

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        /// <response code="200">User retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> GetUser(int id)
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);
            return await _userService.GetUserByIdAsync(id);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> GetAllUsers()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                _logger.LogInformation("Admin user {UserId} requesting all users", userIdClaim);

                var users = await _userService.GetAllUsersAsync();

                _logger.LogInformation("All users retrieved successfully by admin: {UserId}", userIdClaim);

                return new ApiResponse(true, "Users retrieved successfully", users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return new ApiResponse(false, "An error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Get paginated list of users with filtering and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="sortBy">Sort field (username, email, phone, roleId, createdTime)</param>
        /// <param name="sortOrder">Sort direction (asc, desc)</param>
        /// <param name="search">Search by username, full name, email, or phone</param>
        /// <param name="roleId">Filter by role ID</param>
        /// <param name="dateFrom">Filter by creation date from</param>
        /// <param name="dateTo">Filter by creation date to</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<PagedResult<UserProfileDto>> GetUsersPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            _logger.LogInformation("Getting paged users - Page: {Page}, PageSize: {PageSize}, Search: {Search}, RoleId: {RoleId}", page, pageSize, search, roleId);
            
            return await _userService.GetUsersPagedAsync(page, pageSize, sortBy, sortOrder, search, roleId, dateFrom, dateTo);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="dto">User creation data</param>
        /// <returns>Created user details</returns>
        /// <response code="200">User created successfully</response>
        /// <response code="400">Invalid input or username already exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> CreateUser([FromBody] CreateUserDto dto)
        {
            _logger.LogInformation("Creating user: {Username}", dto.Username);
            var adminUsername = GetAdminUsername();
            return await _userService.CreateUserAsync(dto, adminUsername);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">User update data</param>
        /// <returns>Updated user details</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            var adminUsername = GetAdminUsername();
            return await _userService.UpdateUserAsync(id, dto, adminUsername);
        }

        /// <summary>
        /// Delete a user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">User not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> DeleteUser(int id)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            var adminUsername = GetAdminUsername();
            return await _userService.DeleteUserAsync(id, adminUsername);
        }

        #endregion

        #region Password Operations

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Password change data</param>
        /// <returns>Success message</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid input or current password incorrect</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}/password")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            _logger.LogInformation("Changing password for user ID: {UserId}", id);
            var adminUsername = GetAdminUsername();
            return await _userService.ChangePasswordAsync(id, dto, adminUsername);
        }

        /// <summary>
        /// Reset user password (Admin only - no current password required)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Password reset data</param>
        /// <returns>Success message</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid input or passwords do not match</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}/password-reset")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> ResetPassword(int id, [FromBody] PasswordResetDto dto)
        {
            _logger.LogInformation("Resetting password for user ID: {UserId}", id);
            var adminUsername = GetAdminUsername();
            return await _userService.ResetPasswordAsync(id, dto, adminUsername);
        }

        #endregion

        #region Profile Operations

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("profile")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();

                _logger.LogInformation("Getting profile for user: {UserId}", userId);

                var profile = await _userService.GetUserProfileAsync(userId);

                _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userId);

                return Ok(new ApiResponse(true, "Profile retrieved successfully", profile));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("User not found: {Message}", ex.Message);
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new ApiResponse(false, "An error occurred while retrieving profile"));
            }
        }

        #endregion

        #region Private Helper Methods

        private string GetAdminUsername()
        {
            return GetUserIdFromClaims.GetUsername(User);
        }

        private int GetUserId()
        {
            var userIdClaim = GetUserIdFromClaims.GetUserId(User);
            return userIdClaim;
        }

        #endregion
    }
}
