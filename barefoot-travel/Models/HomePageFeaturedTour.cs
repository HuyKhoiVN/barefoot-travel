using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class HomePageFeaturedTour
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int DisplayOrder { get; set; }

    public string? Title { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public string? CardClass { get; set; }

    public bool Active { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual Tour Tour { get; set; } = null!;
}
