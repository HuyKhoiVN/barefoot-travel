using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class HomePageSelectedTour
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public int TourId { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
