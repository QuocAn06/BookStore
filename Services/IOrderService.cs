using BookStore.Models;

namespace BookStore.Services
{
    public interface IOrderService
    {
        // Customer
        Task<PlaceOrderResult> PlaceOrderAsync(string userId);
        Task<Order?> GetOrderForUserAsync(int orderId, string userId);

        // Admin
        Task<IReadOnlyList<Order>> GetAllForAdminAsync();
        Task<Order?> GetDetailForAdminAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}