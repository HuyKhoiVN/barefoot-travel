using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Booking
{
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "Tour ID is required")]
        public int TourId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Number of people is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1")]
        public int People { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(255, ErrorMessage = "Customer name cannot exceed 255 characters")]
        public string NameCustomer { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Payment status is required")]
        [StringLength(50, ErrorMessage = "Payment status cannot exceed 50 characters")]
        public string PaymentStatus { get; set; } = "Pending";

        public int? UserId { get; set; } // Optional for guest bookings
    }
}
