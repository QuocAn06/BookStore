using BookStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class CartController: Controller
    {
        private readonly ICartSessionService _cart;
        private readonly IBookService _bookService;
        public CartController(ICartSessionService cart, IBookService bookService)
        {
            _cart = cart;
            _bookService = bookService;
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
            var book = await _bookService.GetByIdAsync(bookId);
            if (book is null)
                return NotFound();
            if (quantity <= 0)
                quantity = 1;
            _cart.AddToCart(book.Id, book.Title, book.Price, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int bookId)
        {
            _cart.Remove(bookId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int bookId, int quantity)
        {
            _cart.UpdateQuantity(bookId, quantity);
            return RedirectToAction(nameof(Index));
        }
    }
}
