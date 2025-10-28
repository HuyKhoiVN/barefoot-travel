using System;

namespace barefoot_travel.Models;

public partial class TourStatusHistory
{
    public int Id { get; set; }
    
    public int TourId { get; set; }
    
    public string? OldStatus { get; set; }
    
    public string NewStatus { get; set; } = null!;
    
    public string ChangedBy { get; set; } = null!;
    
    public DateTime ChangedTime { get; set; }
    
    public string? Reason { get; set; }
    
    public bool Active { get; set; }
    
    public virtual Tour Tour { get; set; } = null!;
}
