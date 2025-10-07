using System;

namespace barefoot_travel.DTOs.Booking
{
    public class BookingFilterDto
    {
        public int? StatusTypeId { get; set; }
        public int? TourId { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? CreatedTimeFrom { get; set; }
        public DateTime? CreatedTimeTo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NameCustomer { get; set; }
        public string? Email { get; set; }
        public string? PaymentStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedTime";
        public string SortDirection { get; set; } = "desc";
    }
}
