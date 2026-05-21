using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Controllers
{
    [Authorize]
    public class OrderController: Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICartSessionService _cart;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ApplicationDbContext db,
            ICartSessionService cart,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _cart = cart;
            _userManager = userManager;
        }

        // GET /Order/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = _cart.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

        // POST /Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = _cart.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            var userId = _userManager.GetUserId(User);
            if (userId is null)
                return Challenge();

            var bookIds = cart.Items.Select(i => i.ProductId).ToList();
            var books = await _db.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToListAsync();

            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var item in cart.Items)
            {
                var book = books.FirstOrDefault(b => b.Id == item.ProductId);
                if (book is null)
                {
                    ModelState.AddModelError("", $"Book id {item.ProductId} no longer exists.");
                    return View("Checkout", cart);
                }

                if (book.Stock < item.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for \"{book.Title}\".");
                    return View("Checkout", cart);
                }

                order.OrderDetails.Add(new OrderDetail
                {
                    BookId = book.Id,
                    Quantity = item.Quantity,
                    Price = book.Price
                });
            }

            order.TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity);

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var detail in order.OrderDetails)
                {
                    var book = books.First(b => b.Id == detail.BookId);
                    book.Stock -= detail.Quantity;
                }
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            _cart.Clear();
            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order is null)
                return NotFound();
            return View(order);
        }
    }
}
