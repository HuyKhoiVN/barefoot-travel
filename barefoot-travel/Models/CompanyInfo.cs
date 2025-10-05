using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class CompanyInfo
{
    public int Id { get; set; }

    public string? Icon { get; set; }

    public string Title { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
