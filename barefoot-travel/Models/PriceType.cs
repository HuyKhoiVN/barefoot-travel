using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class PriceType
{
    public int Id { get; set; }

    public string PriceTypeName { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
