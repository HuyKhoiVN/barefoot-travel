using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Policy;
using barefoot_travel.Services;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        /// <summary>
        /// Get policy by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            var result = await _policyService.GetPolicyByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get all policies
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPolicies()
        {
            var result = await _policyService.GetAllPoliciesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get policies with pagination
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPoliciesPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? policyType = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] bool? active = null)
        {
            try
            {
                var result = await _policyService.GetPoliciesPagedAsync(page, pageSize, policyType, sortBy, sortOrder, active);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
        }

        /// <summary>
        /// Create new policy
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid model state", ModelState));
            }

            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _policyService.CreatePolicyAsync(dto, adminUsername);
            return result.Success ? CreatedAtAction(nameof(GetPolicyById), new { id = ((PolicyDto)result.Data!).Id }, result) : BadRequest(result);
        }

        /// <summary>
        /// Update policy
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] UpdatePolicyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid model state", ModelState));
            }

            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _policyService.UpdatePolicyAsync(id, dto, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete policy
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _policyService.DeletePolicyAsync(id, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update policy status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePolicyStatus(int id, [FromBody] bool active)
        {
            var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
            var result = await _policyService.UpdatePolicyStatusAsync(id, active, adminUsername);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
