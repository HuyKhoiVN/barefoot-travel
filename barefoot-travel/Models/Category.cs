using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class Category
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string CategoryName { get; set; } = null!;


    public bool Enable { get; set; }

    public string Type { get; set; } = null!;

    public int Priority { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public string? HomepageTitle { get; set; }

    public string? HomepageConfig { get; set; }

    public int? HomepageOrder { get; set; }

    public string? WaysToTravelImage1 { get; set; }

    public string? WaysToTravelImage2 { get; set; }

    public int? WaysToTravelOrder { get; set; }

    public bool? ShowInWaysToTravel { get; set; }

    public string? DailyTourBadge { get; set; }

    public string? DailyTourDescription { get; set; }

    public string? DailyTourImageUrl { get; set; }

    public int? DailyTourOrder { get; set; }

    public bool? ShowInDailyTours { get; set; }

    public string? DailyTourCardClass { get; set; }

    public string? Slug { get; set; }

    public virtual ICollection<HomePageSectionCategory> HomePageSectionCategories { get; set; } = new List<HomePageSectionCategory>();

    public virtual ICollection<HomePageSection> HomePageSections { get; set; } = new List<HomePageSection>();

    public virtual ICollection<HomePageSelectedTour> HomePageSelectedTours { get; set; } = new List<HomePageSelectedTour>();
}
