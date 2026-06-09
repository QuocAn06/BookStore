namespace BookStore.Models
{
    public static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Pending, Processing, Shipped, Completed, Cancelled
        };
    }
}
