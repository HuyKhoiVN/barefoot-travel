using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class Policy
{
    public int Id { get; set; }

    public string PolicyType { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public string Content { get; set; } = null!;
}
