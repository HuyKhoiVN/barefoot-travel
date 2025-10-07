namespace barefoot_travel.Common
{
    public static class RoleConstant
    {
        public const int ADMIN = 1;
        public const int USER = 2;
    }

    public static class PaymentStatusConstant
    {
        public const string PENDING = "pending";
        public const string PAID = "paid";
        public const string CANCELLED = "Cancelled";
    }

    public static class BookingStatusConstant
    {
        public const int Pending = 1;
        public const int Confirmed = 2;
        public const int InProgress = 3;
        public const int Cancel = 4;
        public const int Complete = 5;
    }
}
