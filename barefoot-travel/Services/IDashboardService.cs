using barefoot_travel.Common;
using barefoot_travel.DTOs.Dashboard;

namespace barefoot_travel.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse> GetDashboardStatsAsync(string? filterPeriod = null);
    }
}

