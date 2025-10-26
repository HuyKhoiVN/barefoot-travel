namespace barefoot_travel.DTOs.Category
{
    // DTO for rendering Featured Tour item on homepage
    public class FeaturedTourDto
    {
        public int Id { get; set; }
        public string Badge { get; set; } = string.Empty; // card-category (e.g., "IN THE SPOTLIGHTS")
        public string CategoryName { get; set; } = string.Empty; // h3 title
        public string Description { get; set; } = string.Empty; // p description
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int TourId { get; set; }
        public string? CardClass { get; set; }
    }

    // DTO for Featured Tours config response
    public class FeaturedToursConfigDto
    {
        public List<FeaturedTourDto> Tours { get; set; } = new();
    }

    // DTO for configuring Featured Tour
    public class ConfigureFeaturedTourDto
    {
        public int TourId { get; set; }
        public string Badge { get; set; } = string.Empty; // card-category
        public string CategoryName { get; set; } = string.Empty; // h3 title
        public string Description { get; set; } = string.Empty; // p description
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? CardClass { get; set; }
        public bool Active { get; set; } = true;
    }

    // DTO for rendering Daily Tour item on homepage
    public class DailyTourDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Badge { get; set; } = string.Empty; // daily-card-category (e.g., "DAILY TOURS")
        public string CategoryName { get; set; } = string.Empty; // h3 title
        public string Description { get; set; } = string.Empty; // p description
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? CardClass { get; set; }
    }

    // DTO for Daily Tours config response
    public class DailyToursConfigDto
    {
        public List<DailyTourDto> Tours { get; set; } = new();
    }

    // DTO for configuring Daily Tour
    public class ConfigureDailyTourDto
    {
        public int CategoryId { get; set; }
        public string Badge { get; set; } = string.Empty; // daily-card-category
        public string CategoryName { get; set; } = string.Empty; // h3 title
        public string Description { get; set; } = string.Empty; // p description
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? CardClass { get; set; }
        public bool Active { get; set; } = true;
    }

    // DTO for reordering
    public class ReorderTourDto
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
    }
}
