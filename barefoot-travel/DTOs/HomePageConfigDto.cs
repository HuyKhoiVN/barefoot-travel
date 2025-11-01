using System.Text.Json.Serialization;

namespace barefoot_travel.DTOs
{
    public class HomepageConfigDto
    {
        [JsonPropertyName("layoutStyle")]
        public string LayoutStyle { get; set; } = "grid";
        
        [JsonPropertyName("maxItems")]
        public int MaxItems { get; set; } = 8;
        
        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; } = 0;
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
        
        [JsonPropertyName("badgeText")]
        public string? BadgeText { get; set; }
        
        [JsonPropertyName("customClass")]
        public string? CustomClass { get; set; }
        [JsonPropertyName("spotlightImageUrl")]
        public string? SpotlightImageUrl { get; set; }
        [JsonPropertyName("selectionMode")]
        public string? SelectionMode { get; set; }
    }

    public class ConfigureHomepageDto
    {
        public string HomepageTitle { get; set; } = string.Empty;
        public string LayoutStyle { get; set; } = "grid";
        public int MaxItems { get; set; } = 8;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? BadgeText { get; set; }
        public string? CustomClass { get; set; }
        public string? SpotlightImageUrl { get; set; }
        public List<int> SelectedTourIds { get; set; } = new();
        public string? SelectionMode { get; set; }
    }

    public class HomepageSectionDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string HomepageTitle { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TourCount { get; set; }
        public HomepageConfigDto Config { get; set; } = new();
        public List<HomepageTourDto> Tours { get; set; } = new();
    }

    public class HomepageTourDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal PricePerPerson { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public List<string> Images { get; set; } = new();
    }

    public class HomepageDataDto
    {
        public List<HomepageSectionDto> Sections { get; set; } = new();
    }
}
