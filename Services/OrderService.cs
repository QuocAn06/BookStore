using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartSessionService _cart;

        public OrderService(ApplicationDbContext context, ICartSessionService cart)
        {
            _context = context;
            _cart = cart;
        }

        public async Task<CheckoutValidationResult> ValidateAndSyncCheckoutAsync()
        {
            var cart = _cart.GetCart();
            if (cart.Items.Count == 0)
                return CheckoutValidationResult.Fail(cart, new[] { "Cart is empty." });

            var bookIds = cart.Items.Select(i => i.ProductId).ToList();
            var books = await _context.Books
                .AsNoTracking()
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            var errors = new List<string>();

            // Copy list để vừa duyệt vừa Remove an toàn
            foreach (var item in cart.Items.ToList())
            {
                var book = books.FirstOrDefault(b => b.Id == item.ProductId);

                if (book is null)
                {
                    _cart.Remove(item.ProductId);
                    errors.Add($"Book id {item.ProductId} no longer exists and was removed from cart.");
                    continue;
                }

                if (book.Stock < item.Quantity)
                {
                    errors.Add(
                        $"Not enough stock for \"{book.Title}\". " +
                        $"Chỉ còn {book.Stock} cuốn, giỏ đang có {item.Quantity}.");
                }

                if (item.UnitPrice != book.Price)
                {
                    var oldPrice = item.UnitPrice;
                    _cart.UpdateUnitPrice(item.ProductId, book.Price);
                    errors.Add(
                        $"Price of \"{book.Title}\" changed from {oldPrice:N0} to {book.Price:N0}. " +
                        "Please review checkout.");
                }
            }

            var updatedCart = _cart.GetCart();

            if (errors.Count > 0)
                return CheckoutValidationResult.Fail(updatedCart, errors);

            return CheckoutValidationResult.Ok(updatedCart);
        }

        public async Task<PlaceOrderResult> PlaceOrderAsync(string userId)
        {
            var validation = await ValidateAndSyncCheckoutAsync();
            if (!validation.IsValid)
                return PlaceOrderResult.Fail(validation.Errors.ToArray());

            var cart = validation.Cart;
            if (cart.Items.Count == 0)
                return PlaceOrderResult.Fail("Cart is empty.");

            var bookIds = cart.Items.Select(i => i.ProductId).ToList();
            var books = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatuses.Pending,
                OrderDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var item in cart.Items)
            {
                var book = books.FirstOrDefault(b => b.Id == item.ProductId);
                if (book is null)
                {
                    return PlaceOrderResult.Fail($"Book id {item.ProductId} no longer exists.");
                }

                // Safety re-check (race giữa validate và save)
                if (book.Stock < item.Quantity)
                {
                    return PlaceOrderResult.Fail($"Not enough stock for \"{book.Title}\".");
                }

                if (item.UnitPrice != book.Price)
                {
                    return PlaceOrderResult.Fail(
                        $"Price of \"{book.Title}\" changed from {item.UnitPrice:N0} to {book.Price:N0}. " +
                        "Please review checkout.");
                }

                order.OrderDetails.Add(new OrderDetail
                {
                    BookId = book.Id,
                    Quantity = item.Quantity,
                    Price = book.Price
                });
            }

            order.TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in order.OrderDetails)
                {
                    var book = books.First(b => b.Id == detail.BookId);
                    book.Stock -= detail.Quantity;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            _cart.Clear();
            return PlaceOrderResult.Ok(order.Id);
        }

        public async Task<Order?> GetOrderForUserAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<IReadOnlyList<OrderListItemVM>> GetOrdersForUserAsync(string userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Select(o => new OrderListItemVM
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderDetails.Sum(d => d.Quantity)
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Order>> GetAllForAdminAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .ToListAsync();
        }

        public async Task<Order?> GetDetailForAdminAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null)
                return false;

            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }


    }
}