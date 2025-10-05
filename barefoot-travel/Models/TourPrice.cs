using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class TourPrice
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int PriceTypeId { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
