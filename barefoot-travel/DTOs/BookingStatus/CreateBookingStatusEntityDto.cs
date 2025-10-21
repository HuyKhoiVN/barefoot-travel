using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.BookingStatus
{
    public class CreateBookingStatusEntityDto
    {
        [Required(ErrorMessage = "Status name is required")]
        [StringLength(100, ErrorMessage = "Status name cannot exceed 100 characters")]
        public string StatusName { get; set; } = string.Empty;
    }
}
