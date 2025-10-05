using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class TourImage
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool IsBanner { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
