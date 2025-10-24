using barefoot_travel.DTOs;

namespace barefoot_travel.DTOs.Booking
{
    /// <summary>
    /// DTO for calendar booking display
    /// </summary>
    public class CalendarBookingDto
    {
        public int Id { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int People { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string NameCustomer { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for calendar day with bookings
    /// </summary>
    public class CalendarDayDto
    {
        public DateTime Date { get; set; }
        public List<CalendarBookingDto> Bookings { get; set; } = new List<CalendarBookingDto>();
        public int TotalBookings { get; set; }
        public int TotalPeople { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// DTO for calendar month view
    /// </summary>
    public class CalendarMonthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<CalendarDayDto> Days { get; set; } = new List<CalendarDayDto>();
        public int TotalBookings { get; set; }
        public int TotalPeople { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// DTO for calendar view response
    /// </summary>
    public class CalendarViewDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CalendarMonthDto> Months { get; set; } = new List<CalendarMonthDto>();
        public int TotalBookings { get; set; }
        public int TotalPeople { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
