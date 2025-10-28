using barefoot_travel.DTOs.Category;

namespace barefoot_travel.DTOs.Tour
{
    /// <summary>
    /// DTO for changing tour status
    /// </summary>
    public class ChangeTourStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
    
    /// <summary>
    /// DTO for batch status change
    /// </summary>
    public class BatchChangeTourStatusDto
    {
        public List<int> TourIds { get; set; } = new List<int>();
        public string NewStatus { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
    
    /// <summary>
    /// DTO for tour with status information
    /// </summary>
    public class TourWithStatusDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal PricePerPerson { get; set; }
        public int MaxPeople { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDisplayName { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public string? BannerImageUrl { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
    
    /// <summary>
    /// DTO for status history
    /// </summary>
    public class TourStatusHistoryDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string? OldStatus { get; set; }
        public string? OldStatusDisplayName { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string NewStatusDisplayName { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedTime { get; set; }
        public string? Reason { get; set; }
    }
    
    /// <summary>
    /// DTO for batch delete operation
    /// </summary>
    public class BatchDeleteTourDto
    {
        public List<int> TourIds { get; set; } = new List<int>();
    }
    
    /// <summary>
    /// Response DTO for batch operations
    /// </summary>
    public class BatchOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<int> SuccessfulIds { get; set; } = new List<int>();
        public List<int> FailedIds { get; set; } = new List<int>();
    }
}

