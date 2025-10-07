namespace barefoot_travel.DTOs.Tour
{
    public class TourPolicyResponseDto
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int PolicyId { get; set; }
        public string PolicyType { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
