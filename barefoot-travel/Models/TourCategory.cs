using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class TourCategory
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
