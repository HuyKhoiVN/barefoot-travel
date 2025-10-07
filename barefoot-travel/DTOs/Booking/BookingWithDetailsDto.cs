using System;

namespace barefoot_travel.DTOs.Booking
{
    public class BookingWithDetailsDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserFullName { get; set; }
        public DateTime? StartDate { get; set; }
        public int People { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string NameCustomer { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Note { get; set; }
        public decimal TotalPrice { get; set; }
        public int StatusTypeId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
    }
}
