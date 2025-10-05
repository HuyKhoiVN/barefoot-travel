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
}
