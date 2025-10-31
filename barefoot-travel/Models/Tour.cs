using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class Tour
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? MapLink { get; set; }

    public decimal PricePerPerson { get; set; }

    public int MaxPeople { get; set; }

    public string Duration { get; set; } = null!;

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? ReturnTime { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public string Status { get; set; } = null!;

    public string? Slug { get; set; }

    public virtual ICollection<HomePageFeaturedTour> HomePageFeaturedTours { get; set; } = new List<HomePageFeaturedTour>();

    public virtual ICollection<HomePageSectionTour> HomePageSectionTours { get; set; } = new List<HomePageSectionTour>();

    public virtual ICollection<HomePageSelectedTour> HomePageSelectedTours { get; set; } = new List<HomePageSelectedTour>();
}
