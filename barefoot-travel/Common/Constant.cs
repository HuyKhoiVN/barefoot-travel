namespace barefoot_travel.Common
{
    public static class RoleConstant
    {
        public const int ADMIN = 1;
        public const int USER = 2;
    }

    public static class PaymentStatusConstant
    {
        public const string PENDING = "Pending";
        public const string PAID = "Paid";
        public const string CANCELLED = "Cancelled";
        public const string REFUNDED = "Refunded";
    }

    public static class BookingStatusConstant
    {
        public const int Pending = 1;
        public const int Confirmed = 2;
        public const int InProgress = 3;
        public const int Cancel = 4;
        public const int Complete = 5;
    }

    public static class TourStatusConstant
    {
        public const string Draft = "draft";
        public const string Public = "public";
        public const string Hide = "hide";
        public const string Cancelled = "cancelled";
        
        public static string GetStatusDisplayName(string status)
        {
            return status switch
            {
                "draft" => "Draft",
                "public" => "Public",
                "hide" => "Hidden",
                "cancelled" => "Cancelled",
                _ => "Unknown"
            };
        }
        
        public static bool IsValidStatus(string status)
        {
            return status == Draft || status == Public || status == Hide || status == Cancelled;
        }
        
        public static bool CanTransitionTo(string fromStatus, string toStatus)
        {
            return (fromStatus, toStatus) switch
            {
                ("draft", "public") => true,      // Draft -> Public
                ("draft", "hide") => true,         // Draft -> Hide
                ("draft", "cancelled") => true,    // Draft -> Cancelled
                ("public", "hide") => true,        // Public -> Hide
                ("public", "cancelled") => true,   // Public -> Cancelled
                ("hide", "public") => true,        // Hide -> Public
                ("hide", "cancelled") => true,     // Hide -> Cancelled
                _ => false
            };
        }
        
        public static string GetBadgeClass(string status)
        {
            return status switch
            {
                "draft" => "bg-secondary",
                "public" => "bg-success",
                "hide" => "bg-warning",
                "cancelled" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}
