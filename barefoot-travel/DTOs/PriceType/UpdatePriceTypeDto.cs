using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.PriceType;

public class UpdatePriceTypeDto
{
    [Required(ErrorMessage = "Price type name is required")]
    [StringLength(255, ErrorMessage = "Price type name cannot exceed 255 characters")]
    public string PriceTypeName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Updated by cannot exceed 100 characters")]
    public string? UpdatedBy { get; set; }
}
