namespace barefoot_travel.DTOs.Booking
{
    public class UpdatePaymentStatusDto
    {
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
