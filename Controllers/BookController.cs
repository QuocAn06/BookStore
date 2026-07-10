using BookStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var vm = await _bookService.GetDetailVmAsync(id);
            if (vm is null)
                return NotFound();

            return View(vm);
        }
    }
}