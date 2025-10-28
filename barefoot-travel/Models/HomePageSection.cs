using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class HomePageSection
{
    public int Id { get; set; }

    public string SectionName { get; set; } = null!;

    public string HomepageTitle { get; set; } = null!;

    public string LayoutStyle { get; set; } = null!;

    public int MaxItems { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? BadgeText { get; set; }

    public string? CustomClass { get; set; }

    public string? SpotlightImageUrl { get; set; }

    public string SelectionMode { get; set; } = null!;

    public int? PrimaryCategoryId { get; set; }

    public string? ConfigJson { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<HomePageSectionCategory> HomePageSectionCategories { get; set; } = new List<HomePageSectionCategory>();

    public virtual ICollection<HomePageSectionTour> HomePageSectionTours { get; set; } = new List<HomePageSectionTour>();

    public virtual Category? PrimaryCategory { get; set; }
}
