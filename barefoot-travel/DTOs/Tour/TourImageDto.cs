using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Tour
{
    public class CreateTourImageDto
    {
        [Required(ErrorMessage = "Tour ID is required")]
        public int TourId { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsBanner { get; set; }
    }

    public class UpdateTourImageDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsBanner { get; set; }
    }

    public class TourImageResponseDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsBanner { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
