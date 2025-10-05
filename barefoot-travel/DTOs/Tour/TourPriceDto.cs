using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Tour
{
    public class CreateTourPriceDto
    {
        [Required(ErrorMessage = "Tour ID is required")]
        public int TourId { get; set; }

        [Required(ErrorMessage = "Price type ID is required")]
        public int PriceTypeId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }

    public class UpdateTourPriceDto
    {
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }

    public class TourPriceResponseDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int PriceTypeId { get; set; }
        public string PriceTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
