using BookStore.Models;

namespace BookStore.Services
{
    public interface ICartSessionService
    {
        Cart GetCart();
        void SaveCart(Cart cart);
        void AddToCart(int productId, string name, decimal unitPrice, int quantity);
        void Remove(int productId);
        void UpdateQuantity(int productId, int quantity);
        void Clear();
    }
}
