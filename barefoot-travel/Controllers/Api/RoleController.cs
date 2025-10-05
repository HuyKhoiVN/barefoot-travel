using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs.Role;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <returns>List of all roles</returns>
        /// <response code="200">Roles retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                _logger.LogInformation("Getting all roles");

                var result = await _roleService.GetAllRolesAsync();

                _logger.LogInformation("All roles retrieved successfully");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return StatusCode(500, new ApiResponse(false, "An error occurred while retrieving roles"));
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Role information</returns>
        /// <response code="200">Role retrieved successfully</response>
        /// <response code="400">Invalid role ID</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">Role not found</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            try
            {
                _logger.LogInformation("Getting role with ID: {RoleId}", id);

                var result = await _roleService.GetRoleByIdAsync(id);

                _logger.LogInformation("Role retrieved successfully with ID: {RoleId}", id);

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request getting role: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Role not found: {Message}", ex.Message);
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role with ID: {RoleId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while retrieving role"));
            }
        }

        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="dto">Role creation data</param>
        /// <returns>Created role information</returns>
        /// <response code="200">Role created successfully</response>
        /// <response code="400">Invalid input or role name already exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new role: {RoleName}", dto.RoleName);

                var result = await _roleService.CreateRoleAsync(dto);

                _logger.LogInformation("Role created successfully: {RoleName}", dto.RoleName);

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request creating role: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", dto.RoleName);
                return StatusCode(500, new ApiResponse(false, "An error occurred while creating role"));
            }
        }

        /// <summary>
        /// Update existing role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="dto">Role update data</param>
        /// <returns>Updated role information</returns>
        /// <response code="200">Role updated successfully</response>
        /// <response code="400">Invalid input or role name already exists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">Role not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                _logger.LogInformation("Updating role with ID: {RoleId}", id);

                var result = await _roleService.UpdateRoleAsync(id, dto);

                _logger.LogInformation("Role updated successfully with ID: {RoleId}", id);

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request updating role: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Role not found for update: {Message}", ex.Message);
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while updating role"));
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Role deleted successfully</response>
        /// <response code="400">Invalid role ID</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">Role not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                _logger.LogInformation("Deleting role with ID: {RoleId}", id);

                var result = await _roleService.DeleteRoleAsync(id);

                _logger.LogInformation("Role deleted successfully with ID: {RoleId}", id);

                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning("Bad request deleting role: {Message}", ex.Message);
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Role not found for deletion: {Message}", ex.Message);
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while deleting role"));
            }
        }
    }
}
