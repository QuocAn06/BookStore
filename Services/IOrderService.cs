using BookStore.Models;
using BookStore.Models.ViewModels;

namespace BookStore.Services
{
    public interface IOrderService
    {
        // Customer
        Task<CheckoutValidationResult> ValidateAndSyncCheckoutAsync();
        Task<PlaceOrderResult> PlaceOrderAsync(string userId);
        Task<Order?> GetOrderForUserAsync(int orderId, string userId);
        Task<IReadOnlyList<OrderListItemVM>> GetOrdersForUserAsync(string userId);

        // Admin
        Task<IReadOnlyList<Order>> GetAllForAdminAsync();
        Task<Order?> GetDetailForAdminAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}