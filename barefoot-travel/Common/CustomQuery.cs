namespace barefoot_travel.Common
{
    public static class CustomQuery
    {
        public static string ToCustomString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm:ss");
        }
        public static string ToDateString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }
    }
}
