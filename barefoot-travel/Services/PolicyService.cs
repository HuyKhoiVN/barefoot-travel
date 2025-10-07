using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Policy;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyService(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<ApiResponse> GetPolicyByIdAsync(int id)
        {
            try
            {
                var policy = await _policyRepository.GetByIdAsync(id);
                if (policy == null)
                {
                    return new ApiResponse(false, "Policy not found");
                }

                var policyDto = MapToPolicyDto(policy);
                return new ApiResponse(true, "Policy retrieved successfully", policyDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetAllPoliciesAsync()
        {
            try
            {
                var policies = await _policyRepository.GetAllAsync();
                var policyDtos = policies.Select(MapToPolicyDto).ToList();
                return new ApiResponse(true, "Policies retrieved successfully", policyDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving policies: {ex.Message}");
            }
        }

        public async Task<PagedResult<PolicyDto>> GetPoliciesPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", bool? active = null)
        {
            try
            {
                var pagedResult = await _policyRepository.GetPagedAsync(page, pageSize, sortBy, sortOrder, active);
                var policyDtos = pagedResult.Items.Select(MapToPolicyDto).ToList();

                return new PagedResult<PolicyDto>
                {
                    Items = policyDtos,
                    TotalItems = pagedResult.TotalItems,
                    TotalPages = pagedResult.TotalPages,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged policies: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreatePolicyAsync(CreatePolicyDto dto, string adminUsername)
        {
            try
            {
                // Validate type uniqueness
                if (await _policyRepository.TypeExistsAsync(dto.PolicyType))
                {
                    return new ApiResponse(false, "Policy type already exists");
                }

                var policy = MapToPolicy(dto, adminUsername);
                var createdPolicy = await _policyRepository.CreateAsync(policy);
                var policyDto = MapToPolicyDto(createdPolicy);

                return new ApiResponse(true, "Policy created successfully", policyDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdatePolicyAsync(int id, UpdatePolicyDto dto, string adminUsername)
        {
            try
            {
                var policy = await _policyRepository.GetByIdAsync(id);
                if (policy == null)
                {
                    return new ApiResponse(false, "Policy not found");
                }

                // Validate type uniqueness (excluding current policy)
                if (await _policyRepository.TypeExistsAsync(dto.PolicyType, id))
                {
                    return new ApiResponse(false, "Policy type already exists");
                }

                MapToPolicyForUpdate(policy, dto, adminUsername);
                await _policyRepository.UpdateAsync(policy);
                var policyDto = MapToPolicyDto(policy);

                return new ApiResponse(true, "Policy updated successfully", policyDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeletePolicyAsync(int id, string adminUsername)
        {
            try
            {
                var success = await _policyRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Policy not found");
                }

                return new ApiResponse(true, "Policy deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdatePolicyStatusAsync(int id, bool active, string adminUsername)
        {
            try
            {
                var success = await _policyRepository.UpdateStatusAsync(id, active, adminUsername);
                if (!success)
                {
                    return new ApiResponse(false, "Policy not found");
                }

                var statusDto = new PolicyStatusDto { Id = id, Active = active, UpdatedTime = DateTime.UtcNow };
                return new ApiResponse(true, "Policy status updated successfully", statusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating policy status: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private Policy MapToPolicy(CreatePolicyDto dto, string adminUsername)
        {
            return new Policy
            {
                PolicyType = dto.PolicyType,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = adminUsername,
                Active = true
            };
        }

        private void MapToPolicyForUpdate(Policy policy, UpdatePolicyDto dto, string adminUsername)
        {
            policy.PolicyType = dto.PolicyType;
            policy.UpdatedTime = DateTime.UtcNow;
            policy.UpdatedBy = adminUsername;
        }

        private PolicyDto MapToPolicyDto(Policy policy)
        {
            return new PolicyDto
            {
                Id = policy.Id,
                PolicyType = policy.PolicyType,
                CreatedTime = policy.CreatedTime,
                UpdatedTime = policy.UpdatedTime,
                UpdatedBy = policy.UpdatedBy,
                Active = policy.Active
            };
        }

        #endregion
    }
}
