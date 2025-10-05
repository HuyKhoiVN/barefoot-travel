using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
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
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID in token");
                    return Unauthorized(new ApiResponse(false, "Invalid user token"));
                }

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

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <returns>List of all users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                _logger.LogInformation("Admin user {UserId} requesting all users", userIdClaim);

                var users = await _userService.GetAllUsersAsync();

                _logger.LogInformation("All users retrieved successfully by admin: {UserId}", userIdClaim);

                return Ok(new ApiResponse(true, "Users retrieved successfully", users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new ApiResponse(false, "An error occurred while retrieving users"));
            }
        }
    }
}
