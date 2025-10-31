using System.ComponentModel.DataAnnotations;

namespace barefoot_travel.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Enable { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
        public int TotalChild { get; set; }
        public int TotalTours { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
        public string? Slug { get; set; }

        public int? ParentId { get; set; }

        public bool Enable { get; set; } = true;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        public int Priority { get; set; } = 0;
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
        public string? Slug { get; set; }

        public int? ParentId { get; set; }

        public bool Enable { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        public int Priority { get; set; }
    }

    public class CategoryStatusDto
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }

    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Enable { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
        public int TotalTours { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new List<CategoryTreeDto>();
    }
}
