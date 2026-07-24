using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartSessionService _cart;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ICartSessionService cart,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _cart = cart;
            _orderService = orderService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = _cart.GetCart();
            if (cart.Items.Count == 0)
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

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

            var result = await _orderService.PlaceOrderAsync(userId);

            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);

                return View("Checkout", cart);
            }

            return RedirectToAction(nameof(Success), new { id = result.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
                return Challenge();

            var order = await _orderService.GetOrderForUserAsync(id, userId);
            if (order is null)
                return NotFound();

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
                return Challenge();

            var orders = await _orderService.GetOrdersForUserAsync(userId);
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId is null)
                return Challenge();

            var order = await _orderService.GetOrderForUserAsync(id, userId);
            if (order is null)
                return NotFound();

            var vm = new OrderDetailVM
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Lines = order.OrderDetails
                    .Select(d => new OrderDetailLineVM
                    {
                        BookTitle = d.Book?.Title ?? "—",
                        Price = d.Price,
                        Quantity = d.Quantity
                    })
                    .ToList()
            };

            return View(vm);
        }
    }
}