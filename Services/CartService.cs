using BookStore.Models;

namespace BookStore.Services
{
    public class CartService : ICartService
    {
        private readonly ICartSessionService _cartSession;
        private readonly IBookService _bookService;

        public CartService(ICartSessionService cartSession, IBookService bookService)
        {
            _cartSession = cartSession;
            _bookService = bookService;
        }

        public async Task<CartOperationResult> TryAddAsync(int bookId, int quantity)
        {
            if (quantity <= 0)
                quantity = 1;

            var book = await _bookService.GetByIdAsync(bookId);
            if (book is null)
                return CartOperationResult.Fail("Sách không tồn tại.");

            if (book.Stock <= 0)
                return CartOperationResult.Fail($"\"{book.Title}\" đã hết hàng.");

            var cart = _cartSession.GetCart();
            var existingQty = cart.Items
                .FirstOrDefault(x => x.ProductId == bookId)
                ?.Quantity ?? 0;

            var newTotal = existingQty + quantity;
            if (newTotal > book.Stock)
            {
                return CartOperationResult.Fail(
                    $"Chỉ còn {book.Stock} cuốn \"{book.Title}\". " +
                    $"Bạn đã có {existingQty} cuốn trong giỏ.");
            }

            _cartSession.AddToCart(book.Id, book.Title, book.Price, quantity);
            return CartOperationResult.Ok();
        }

        public async Task<CartOperationResult> TryUpdateQuantityAsync(int bookId, int quantity)
        {
            var cart = _cartSession.GetCart();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == bookId);
            if (item is null)
                return CartOperationResult.Ok();

            if (quantity <= 0)
            {
                _cartSession.UpdateQuantity(bookId, quantity);
                return CartOperationResult.Ok();
            }

            var book = await _bookService.GetByIdAsync(bookId);
            if (book is null)
            {
                _cartSession.Remove(bookId);
                return CartOperationResult.Fail(
                    "Sách không còn tồn tại và đã được xóa khỏi giỏ hàng.");
            }

            if (quantity > book.Stock)
            {
                return CartOperationResult.Fail(
                    $"Chỉ còn {book.Stock} cuốn \"{book.Title}\".");
            }

            _cartSession.UpdateQuantity(bookId, quantity);
            return CartOperationResult.Ok();
        }
    }
}