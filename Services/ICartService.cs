namespace BookStore.Services
{
    public interface ICartService
    {
        Task<CartOperationResult> TryAddAsync(int bookId, int quantity);
        Task<CartOperationResult> TryUpdateQuantityAsync(int bookId, int quantity);
    }
}