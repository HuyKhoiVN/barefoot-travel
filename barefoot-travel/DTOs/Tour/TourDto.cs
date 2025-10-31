namespace barefoot_travel.DTOs.Tour
{
    public class TourDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MapLink { get; set; }
        public decimal PricePerPerson { get; set; }
        public int MaxPeople { get; set; }
        public string Duration { get; set; } = string.Empty;
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? ReturnTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
        public List<TourImageDto> Images { get; set; } = new List<TourImageDto>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }

    public class TourDetailDto : TourDto
    {
        // Images and Categories inherited from TourDto
        public List<TourPriceDto> Prices { get; set; } = new List<TourPriceDto>();
        public List<PolicyDto> Policies { get; set; } = new List<PolicyDto>();
    }

    public class TourImageDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsBanner { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdatedBy { get; set; }
        public bool Active { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class TourPriceDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int PriceTypeId { get; set; }
        public string PriceTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class PolicyDto
    {
        public int Id { get; set; }
        public string PolicyType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class TourStatusDto
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }

    public class TourItineraryDto
    {
        public int Id { get; set; }
        public string ItineraryJson { get; set; } = string.Empty;
        public DateTime? UpdatedTime { get; set; }
    }

    public class MarketingTagDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
