namespace barefoot_travel.DTOs.Tour
{
    public class TourPriceResponseDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int PriceTypeId { get; set; }
        public string PriceTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
