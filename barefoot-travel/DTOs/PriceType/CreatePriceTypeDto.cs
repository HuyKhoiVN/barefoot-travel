using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.PriceType;

public class CreatePriceTypeDto
{
    [Required(ErrorMessage = "Price type name is required")]
    [StringLength(255, ErrorMessage = "Price type name cannot exceed 255 characters")]
    public string PriceTypeName { get; set; } = string.Empty;
}
