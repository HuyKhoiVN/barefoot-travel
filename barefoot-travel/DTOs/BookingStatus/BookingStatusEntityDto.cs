using System;

namespace barefoot_travel.DTOs.BookingStatus
{
    public class BookingStatusEntityDto
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
    }
}
