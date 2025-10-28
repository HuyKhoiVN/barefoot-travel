using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Section
{
    public class ConfigureHomePageSectionDto
    {
        [Required]
        public string SectionName { get; set; } = string.Empty;
        
        [Required]
        public string HomepageTitle { get; set; } = string.Empty;
        
        public string LayoutStyle { get; set; } = "grid";
        
        public int MaxItems { get; set; } = 8;
        
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public string? BadgeText { get; set; }
        
        public string? CustomClass { get; set; }
        
        public string? SpotlightImageUrl { get; set; }
        
        [Required]
        public string SelectionMode { get; set; } = "auto";
        
        /// <summary>
        /// For auto mode: Single category ID that automatically fetches tours
        /// </summary>
        public int? PrimaryCategoryId { get; set; }
        
        /// <summary>
        /// For manual mode: Multiple category IDs to choose tours from
        /// </summary>
        public List<int>? CategoryIds { get; set; }
        
        /// <summary>
        /// For manual mode: Manually selected tour IDs
        /// </summary>
        public List<int>? SelectedTourIds { get; set; }
    }
}

