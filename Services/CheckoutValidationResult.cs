using BookStore.Models;

namespace BookStore.Services
{
    public class CheckoutValidationResult
    {
        public bool IsValid { get; init; }
        public Cart Cart { get; init; } = new();
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static CheckoutValidationResult Ok(Cart cart) => new()
        {
            IsValid = true,
            Cart = cart
        };

        public static CheckoutValidationResult Fail(Cart cart, IEnumerable<string> errors) => new()
        {
            IsValid = false,
            Cart = cart,
            Errors = errors.ToList()
        };
    }
}