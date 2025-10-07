using System;

namespace barefoot_travel.DTOs.Booking
{
    public class ExportBookingDto
    {
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? CreatedTimeFrom { get; set; }
        public DateTime? CreatedTimeTo { get; set; }
        public int? StatusTypeId { get; set; }
        public int? TourId { get; set; }
        public string ExportFormat { get; set; } = "Excel"; // Excel or PDF
    }
}
