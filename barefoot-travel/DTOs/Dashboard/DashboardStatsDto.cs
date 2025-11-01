namespace barefoot_travel.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard statistics data
    /// </summary>
    public class DashboardStatsDto
    {
        public MonthlyOverviewDto MonthlyOverview { get; set; } = new();
        public WeeklyStatsDto WeeklyStats { get; set; } = new();
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
    }

    /// <summary>
    /// Monthly booking overview for chart
    /// </summary>
    public class MonthlyOverviewDto
    {
        public List<MonthlyDataPoint> ThisMonth { get; set; } = new();
        public List<MonthlyDataPoint> LastMonth { get; set; } = new();
        public int ThisMonthTotal { get; set; }
        public int LastMonthTotal { get; set; }
        public decimal PercentageChange { get; set; }
    }

    public class MonthlyDataPoint
    {
        public string Date { get; set; } = string.Empty;
        public int Bookings { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Weekly statistics
    /// </summary>
    public class WeeklyStatsDto
    {
        public TopTourDto? TopBookings { get; set; }
        public TopTourDto? BestRated { get; set; }
        public TopTourDto? MostPopular { get; set; }
        public RevenueDto Revenue { get; set; } = new();
    }

    public class TopTourDto
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Rating { get; set; }
        public string ChangePercentage { get; set; } = "0%";
    }

    public class RevenueDto
    {
        public decimal TotalRevenue { get; set; }
        public string ChangePercentage { get; set; } = "0%";
    }

    /// <summary>
    /// Recent booking for dashboard table
    /// </summary>
    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}

