using barefoot_travel.Common;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    /// <summary>
    /// Dashboard statistics API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics including monthly overview, weekly stats, and recent bookings
        /// </summary>
        /// <param name="filterPeriod">Filter period: 1=This Month, 2=Last Month, 3=This Year (for recent bookings table)</param>
        /// <returns>Dashboard statistics data</returns>
        [HttpGet("stats")]
        public async Task<ApiResponse> GetDashboardStats([FromQuery] string? filterPeriod = null)
        {
            _logger.LogInformation("Getting dashboard statistics with filter: {FilterPeriod}", filterPeriod ?? "All");

            try
            {
                return await _dashboardService.GetDashboardStatsAsync(filterPeriod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}

