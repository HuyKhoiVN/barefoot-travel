namespace barefoot_travel.DTOs.PriceType;

public class PriceTypeDto
{
    public int Id { get; set; }
    public string PriceTypeName { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public bool Active { get; set; }
}
