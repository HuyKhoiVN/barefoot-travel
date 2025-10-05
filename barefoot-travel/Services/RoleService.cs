using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs.Role;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ApiResponse> GetRoleByIdAsync(int id)
        {
            // Validation in Service layer
            if (id <= 0)
            {
                throw new BadRequestException("Invalid role ID");
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new NotFoundException("Role not found");
            }

            var roleDto = MapToRoleDto(role);
            return new ApiResponse(true, "Role retrieved successfully", roleDto);
        }

        public async Task<ApiResponse> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            var roleDtos = roles.Select(MapToRoleDto).ToList();

            return new ApiResponse(true, "Roles retrieved successfully", roleDtos);
        }

        public async Task<ApiResponse> CreateRoleAsync(CreateRoleDto dto)
        {
            // Validation in Service layer
            if (string.IsNullOrWhiteSpace(dto.RoleName))
            {
                throw new BadRequestException("Role name is required");
            }

            if (dto.RoleName.Length > 100)
            {
                throw new BadRequestException("Role name cannot exceed 100 characters");
            }

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 255)
            {
                throw new BadRequestException("Description cannot exceed 255 characters");
            }

            // Check if role name already exists
            var existingRole = await _roleRepository.GetByNameAsync(dto.RoleName);
            if (existingRole != null)
            {
                throw new BadRequestException("Role name already exists");
            }

            var role = MapToRole(dto);
            var createdRole = await _roleRepository.CreateAsync(role);
            var roleDto = MapToRoleDto(createdRole);

            return new ApiResponse(true, "Role created successfully", roleDto);
        }

        public async Task<ApiResponse> UpdateRoleAsync(int id, UpdateRoleDto dto)
        {
            // Validation in Service layer
            if (id <= 0)
            {
                throw new BadRequestException("Invalid role ID");
            }

            if (string.IsNullOrWhiteSpace(dto.RoleName))
            {
                throw new BadRequestException("Role name is required");
            }

            if (dto.RoleName.Length > 100)
            {
                throw new BadRequestException("Role name cannot exceed 100 characters");
            }

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 255)
            {
                throw new BadRequestException("Description cannot exceed 255 characters");
            }

            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null)
            {
                throw new NotFoundException("Role not found");
            }

            // Check if role name already exists (excluding current role)
            var roleWithSameName = await _roleRepository.GetByNameAsync(dto.RoleName);
            if (roleWithSameName != null && roleWithSameName.Id != id)
            {
                throw new BadRequestException("Role name already exists");
            }

            MapToRoleForUpdate(existingRole, dto);
            var updatedRole = await _roleRepository.UpdateAsync(existingRole);
            var roleDto = MapToRoleDto(updatedRole);

            return new ApiResponse(true, "Role updated successfully", roleDto);
        }

        public async Task<ApiResponse> DeleteRoleAsync(int id)
        {
            // Validation in Service layer
            if (id <= 0)
            {
                throw new BadRequestException("Invalid role ID");
            }

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new NotFoundException("Role not found");
            }

            // Check if role is being used by any accounts
            // This would require additional logic to check Account table
            // For now, we'll allow deletion

            var deleted = await _roleRepository.DeleteAsync(id);
            if (!deleted)
            {
                throw new BadRequestException("Failed to delete role");
            }

            return new ApiResponse(true, "Role deleted successfully");
        }

        // Manual mapping methods
        private RoleDto MapToRoleDto(Role role)
        {
            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description,
                CreatedTime = role.CreatedTime,
                UpdatedTime = role.UpdatedTime,
                UpdatedBy = role.UpdatedBy,
                Active = role.Active
            };
        }

        private Role MapToRole(CreateRoleDto dto)
        {
            if (dto == null) return null;

            return new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description,
                CreatedTime = DateTime.UtcNow,
                Active = true
            };
        }

        private void MapToRoleForUpdate(Role existingRole, UpdateRoleDto dto)
        {
            if (existingRole == null || dto == null) return;

            existingRole.RoleName = dto.RoleName;
            existingRole.Description = dto.Description;
            existingRole.Active = dto.Active;
            existingRole.UpdatedTime = DateTime.UtcNow;
        }
    }
}
