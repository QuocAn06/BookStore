namespace BookStore.Services
{
    public class DeleteResult
    {
        public bool Success { get; init; }
        public bool NotFound { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static DeleteResult Ok() => new()
        {
            Success = true
        };

        public static DeleteResult NotFoundResult() => new()
        {
            NotFound = true
        };

        public static DeleteResult Fail(params string[] errors) => new()
        {
            Success = false,
            Errors = errors
        };
    }
}