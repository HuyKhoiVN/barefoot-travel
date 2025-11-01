using barefoot_travel.DTOs.Dashboard;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly SysDbContext _context;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(SysDbContext context, ILogger<DashboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MonthlyOverviewDto> GetMonthlyOverviewAsync(DateTime now)
        {
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            // This month bookings - using from...select with join
            var thisMonthBookings = await (
                from b in _context.Bookings
                where b.CreatedTime >= thisMonthStart && b.Active
                group b by b.CreatedTime.Date into g
                select new MonthlyDataPoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Bookings = g.Count(),
                    Revenue = g.Sum(b => b.TotalPrice)
                }
            ).ToListAsync();

            // Last month bookings
            var lastMonthBookings = await (
                from b in _context.Bookings
                where b.CreatedTime >= lastMonthStart && b.CreatedTime <= lastMonthEnd && b.Active
                group b by b.CreatedTime.Date into g
                select new MonthlyDataPoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Bookings = g.Count(),
                    Revenue = g.Sum(b => b.TotalPrice)
                }
            ).ToListAsync();

            var thisMonthTotal = thisMonthBookings.Sum(b => b.Bookings);
            var lastMonthTotal = lastMonthBookings.Sum(b => b.Bookings);

            var percentageChange = lastMonthTotal > 0
                ? Math.Round(((decimal)(thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100, 2)
                : 0;

            return new MonthlyOverviewDto
            {
                ThisMonth = thisMonthBookings,
                LastMonth = lastMonthBookings,
                ThisMonthTotal = thisMonthTotal,
                LastMonthTotal = lastMonthTotal,
                PercentageChange = percentageChange
            };
        }

        public async Task<WeeklyStatsDto> GetWeeklyStatsAsync(DateTime now)
        {
            var weekStart = now.AddDays(-7);
            var previousWeekStart = weekStart.AddDays(-7);

            // Top bookings this week - using from...select with join
            var topBookings = await (
                from b in _context.Bookings
                join t in _context.Tours on b.TourId equals t.Id
                where b.CreatedTime >= weekStart && b.Active
                group b by new { b.TourId, t.Title } into g
                orderby g.Count() descending
                select new
                {
                    TourId = g.Key.TourId,
                    TourName = g.Key.Title,
                    BookingCount = g.Count()
                }
            ).FirstOrDefaultAsync();

            // Previous week count for comparison
            var topBookingsPrevWeek = topBookings != null ? await (
                from b in _context.Bookings
                where b.TourId == topBookings.TourId &&
                      b.CreatedTime >= previousWeekStart &&
                      b.CreatedTime < weekStart &&
                      b.Active
                select b
            ).CountAsync() : 0;

            var topBookingsChange = topBookingsPrevWeek > 0
                ? Math.Round(((decimal)(topBookings?.BookingCount ?? 0) - topBookingsPrevWeek) / topBookingsPrevWeek * 100, 0)
                : (topBookings?.BookingCount ?? 0) > 0 ? 100 : 0;

            // Most popular (most viewed/booked)
            var mostPopular = await (
                from b in _context.Bookings
                join t in _context.Tours on b.TourId equals t.Id
                where b.CreatedTime >= weekStart && b.Active
                group b by new { b.TourId, t.Title } into g
                orderby g.Count() descending
                select new
                {
                    TourId = g.Key.TourId,
                    TourName = g.Key.Title,
                    BookingCount = g.Count()
                }
            ).FirstOrDefaultAsync();

            var mostPopularPrevWeek = mostPopular != null ? await (
                from b in _context.Bookings
                where b.TourId == mostPopular.TourId &&
                      b.CreatedTime >= previousWeekStart &&
                      b.CreatedTime < weekStart &&
                      b.Active
                select b
            ).CountAsync() : 0;

            var mostPopularChange = mostPopularPrevWeek > 0
                ? Math.Round(((decimal)(mostPopular?.BookingCount ?? 0) - mostPopularPrevWeek) / mostPopularPrevWeek * 100, 0)
                : (mostPopular?.BookingCount ?? 0) > 0 ? 100 : 0;

            // Revenue this week
            var thisWeekRevenue = await (
                from b in _context.Bookings
                where b.CreatedTime >= weekStart && b.Active
                select b.TotalPrice
            ).SumAsync();

            var lastWeekRevenue = await (
                from b in _context.Bookings
                where b.CreatedTime >= previousWeekStart && b.CreatedTime < weekStart && b.Active
                select b.TotalPrice
            ).SumAsync();

            var revenueChange = lastWeekRevenue > 0
                ? Math.Round(((thisWeekRevenue - lastWeekRevenue) / lastWeekRevenue) * 100, 0)
                : thisWeekRevenue > 0 ? 100 : 0;

            return new WeeklyStatsDto
            {
                TopBookings = topBookings != null ? new TopTourDto
                {
                    TourId = topBookings.TourId,
                    TourName = topBookings.TourName ?? "N/A",
                    BookingCount = topBookings.BookingCount,
                    ChangePercentage = topBookingsChange >= 0 ? $"+{topBookingsChange}%" : $"{topBookingsChange}%"
                } : null,
                BestRated = null, // Note: Requires rating system implementation
                MostPopular = mostPopular != null ? new TopTourDto
                {
                    TourId = mostPopular.TourId,
                    TourName = mostPopular.TourName ?? "N/A",
                    BookingCount = mostPopular.BookingCount,
                    ChangePercentage = mostPopularChange >= 0 ? $"+{mostPopularChange}%" : $"{mostPopularChange}%"
                } : null,
                Revenue = new RevenueDto
                {
                    TotalRevenue = thisWeekRevenue,
                    ChangePercentage = revenueChange >= 0 ? $"+{revenueChange}%" : $"{revenueChange}%"
                }
            };
        }

        public async Task<List<RecentBookingDto>> GetRecentBookingsAsync(string? filterPeriod, DateTime now)
        {
            var query = from b in _context.Bookings
                        join t in _context.Tours on b.TourId equals t.Id
                        join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id
                        where b.Active
                        select new
                        {
                            b.Id,
                            b.NameCustomer,
                            b.Email,
                            TourTitle = t.Title,
                            StatusName = bs.StatusName,
                            b.TotalPrice,
                            b.CreatedTime
                        };

            // Apply filter period
            if (!string.IsNullOrEmpty(filterPeriod))
            {
                query = filterPeriod switch
                {
                    "1" => query.Where(b => b.CreatedTime >= new DateTime(now.Year, now.Month, 1)), // This Month
                    "2" => query.Where(b => b.CreatedTime >= new DateTime(now.Year, now.Month, 1).AddMonths(-1) &&
                                           b.CreatedTime < new DateTime(now.Year, now.Month, 1)), // Last Month
                    "3" => query.Where(b => b.CreatedTime >= new DateTime(now.Year, 1, 1)), // This Year
                    _ => query
                };
            }

            var bookings = await (
                from b in query
                orderby b.CreatedTime descending
                select new RecentBookingDto
                {
                    Id = b.Id,
                    CustomerName = b.NameCustomer ?? "N/A",
                    Email = b.Email ?? "N/A",
                    TourName = b.TourTitle ?? "N/A",
                    Status = b.StatusName ?? "Unknown",
                    StatusClass = GetStatusClass(b.StatusName ?? "Unknown"),
                    Amount = b.TotalPrice,
                    CreatedTime = b.CreatedTime
                }
            ).Take(10).ToListAsync();

            return bookings;
        }

        private static string GetStatusClass(string status)
        {
            // Match status classes with booking management
            return status.ToLower() switch
            {
                "pending" => "bg-warning",
                "confirmed" => "bg-success",
                "in progress" => "bg-info",
                "cancel" => "bg-danger",
                "complete" => "bg-info",
                _ => "bg-secondary"
            };
        }
    }
}

