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
}
