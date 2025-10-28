namespace barefoot_travel.DTOs.Section
{
    public class HomePageSectionDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string HomepageTitle { get; set; } = string.Empty;
        public string LayoutStyle { get; set; } = "grid";
        public int MaxItems { get; set; } = 8;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? BadgeText { get; set; }
        public string? CustomClass { get; set; }
        public string? SpotlightImageUrl { get; set; }
        public string SelectionMode { get; set; } = "auto";
        
        // For auto mode - single category
        public int? PrimaryCategoryId { get; set; }
        public string? PrimaryCategoryName { get; set; }
        
        // For manual mode - multiple categories
        public List<int> CategoryIds { get; set; } = new();
        public List<string> CategoryNames { get; set; } = new();
        
        // Tours data
        public List<HomepageTourDto> SelectedTours { get; set; } = new();
        public int TourCount { get; set; }
        
        // Metadata
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

