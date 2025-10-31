namespace barefoot_travel.DTOs.Category
{
    public class WaysToTravelCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int TotalTours { get; set; }
        public string ImageUrl1 { get; set; } = string.Empty;
        public string? ImageUrl2 { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ConfigureWaysToTravelDto
    {
        public string ImageUrl1 { get; set; } = string.Empty;
        public string? ImageUrl2 { get; set; }
        public int DisplayOrder { get; set; }
        public bool ShowInWaysToTravel { get; set; }
    }

    public class WaysToTravelConfigDto
    {
        public List<WaysToTravelCategoryDto> Categories { get; set; } = new();
    }
}
