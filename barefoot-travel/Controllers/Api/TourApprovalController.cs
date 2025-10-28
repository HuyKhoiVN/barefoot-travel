using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/tour/approval")]
    [Authorize]
    public class TourApprovalController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<TourApprovalController> _logger;

        public TourApprovalController(ITourService tourService, ILogger<TourApprovalController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated tours filtered by status
        /// </summary>
        /// <param name="status">Filter by status ('draft', 'public', 'hide', 'cancelled')</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="sortBy">Sort field (title, status, createdtime)</param>
        /// <param name="sortOrder">Sort direction (asc, desc)</param>
        /// <returns>Paginated list of tours with status</returns>
        [HttpGet("paged")]
        public async Task<PagedResult<TourWithStatusDto>> GetToursPagedByStatus(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortOrder = "asc")
        {
            _logger.LogInformation("Getting paged tours by status - Status: {Status}, Page: {Page}, PageSize: {PageSize}", 
                status, page, pageSize);
            
            return await _tourService.GetToursPagedByStatusAsync(status, page, pageSize, sortBy, sortOrder);
        }

        /// <summary>
        /// Change tour status
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="dto">Status change data</param>
        /// <returns>Success or error response</returns>
        [HttpPut("{id}/status")]
        public async Task<ApiResponse> ChangeStatus(int id, [FromBody] ChangeTourStatusDto dto)
        {
            _logger.LogInformation("Changing status for tour {TourId} to {NewStatus}", id, dto.NewStatus);
            var username = GetUserIdFromClaims.GetUsername(User);
            return await _tourService.ChangeStatusAsync(id, dto.NewStatus, username, dto.Reason);
        }

        /// <summary>
        /// Batch change tour status
        /// </summary>
        /// <param name="dto">Batch status change data</param>
        /// <returns>Batch operation result</returns>
        [HttpPut("batch-status")]
        public async Task<ApiResponse> BatchChangeStatus([FromBody] BatchChangeTourStatusDto dto)
        {
            _logger.LogInformation("Batch changing status for {Count} tours to {NewStatus}", 
                dto.TourIds.Count, dto.NewStatus);
            var username = GetUserIdFromClaims.GetUsername(User);
            return await _tourService.BatchChangeStatusAsync(dto.TourIds, dto.NewStatus, username, dto.Reason);
        }

        /// <summary>
        /// Batch delete tours (soft delete)
        /// </summary>
        /// <param name="dto">Batch delete data</param>
        /// <returns>Batch operation result</returns>
        [HttpDelete("batch-delete")]
        public async Task<ApiResponse> BatchDelete([FromBody] BatchDeleteTourDto dto)
        {
            _logger.LogInformation("Batch deleting {Count} tours", dto.TourIds.Count);
            var username = GetUserIdFromClaims.GetUsername(User);
            return await _tourService.BatchDeleteToursAsync(dto.TourIds, username);
        }

        /// <summary>
        /// Get status change history for a tour
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <returns>List of status history</returns>
        [HttpGet("{id}/history")]
        public async Task<ApiResponse> GetStatusHistory(int id)
        {
            _logger.LogInformation("Getting status history for tour {TourId}", id);
            return await _tourService.GetStatusHistoryAsync(id);
        }
    }
}

