using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class HomePageSectionTour
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public int TourId { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public virtual HomePageSection Section { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
