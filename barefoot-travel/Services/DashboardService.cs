using barefoot_travel.Common;
using barefoot_travel.DTOs.Dashboard;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IDashboardRepository dashboardRepository, ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<ApiResponse> GetDashboardStatsAsync(string? filterPeriod = null)
        {
            try
            {
                var now = DateTime.Now;
                var stats = new DashboardStatsDto();

                // Get monthly overview
                stats.MonthlyOverview = await _dashboardRepository.GetMonthlyOverviewAsync(now);

                // Get weekly stats
                stats.WeeklyStats = await _dashboardRepository.GetWeeklyStatsAsync(now);

                // Get recent bookings
                stats.RecentBookings = await _dashboardRepository.GetRecentBookingsAsync(filterPeriod, now);

                return new ApiResponse(true, "Dashboard statistics retrieved successfully", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}

