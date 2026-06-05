namespace BookStore.Services
{
    public class PlaceOrderResult
    {
        public bool Success { get; init; }
        public int? OrderId { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static PlaceOrderResult Ok(int orderId) => new()
        {
            Success = true,
            OrderId = orderId
        };

        public static PlaceOrderResult Fail(params string[] errors) => new()
        {
            Success = false,
            Errors = errors
        };
    }
}