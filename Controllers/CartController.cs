using BookStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartSessionService _cart;
        private readonly ICartService _cartService;

        public CartController(ICartSessionService cart, ICartService cartService)
        {
            _cart = cart;
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = _cart.GetCart();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int bookId, int quantity = 1)
        {
            var result = await _cartService.TryAddAsync(bookId, quantity);

            if (!result.Success)
            {
                TempData["error"] = result.Errors.FirstOrDefault()
                    ?? "Không thể thêm sách vào giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            TempData["success"] = "Đã thêm sách vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int bookId)
        {
            _cart.Remove(bookId);
            TempData["success"] = "Đã xóa sách khỏi giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int bookId, int quantity)
        {
            var result = await _cartService.TryUpdateQuantityAsync(bookId, quantity);

            if (!result.Success)
            {
                TempData["error"] = result.Errors.FirstOrDefault()
                    ?? "Không thể cập nhật số lượng.";
                return RedirectToAction(nameof(Index));
            }

            TempData["success"] = "Đã cập nhật giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }
    }
}