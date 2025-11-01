using barefoot_travel.DTOs.Dashboard;

namespace barefoot_travel.Repositories
{
    public interface IDashboardRepository
    {
        Task<MonthlyOverviewDto> GetMonthlyOverviewAsync(DateTime now);
        Task<WeeklyStatsDto> GetWeeklyStatsAsync(DateTime now);
        Task<List<RecentBookingDto>> GetRecentBookingsAsync(string? filterPeriod, DateTime now);
    }
}

