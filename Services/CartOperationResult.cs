namespace BookStore.Services
{
    public class CartOperationResult
    {
        public bool Success { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static CartOperationResult Ok() => new()
        {
            Success = true
        };

        public static CartOperationResult Fail(params string[] errors) => new()
        {
            Success = false,
            Errors = errors
        };
    }
}