using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Tour
{
    public class CreateTourDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Map link cannot exceed 500 characters")]
        public string? MapLink { get; set; }

        [Required(ErrorMessage = "Price per person is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerPerson { get; set; }

        [Required(ErrorMessage = "Max people is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Max people must be at least 1")]
        public int MaxPeople { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [StringLength(50, ErrorMessage = "Duration cannot exceed 50 characters")]
        public string Duration { get; set; } = string.Empty;

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? ReturnTime { get; set; }

        public List<CreateTourImageDto> Images { get; set; } = new List<CreateTourImageDto>();

        public List<int> Categories { get; set; } = new List<int>();

        public List<CreateTourPriceDto> Prices { get; set; } = new List<CreateTourPriceDto>();

        public List<int> Policies { get; set; } = new List<int>();
    }
}
