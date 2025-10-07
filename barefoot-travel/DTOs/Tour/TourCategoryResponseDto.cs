namespace barefoot_travel.DTOs.Tour
{
    public class TourCategoryResponseDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
