using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    public class BooksController: AdminControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public async Task<IActionResult> Index(string? title, int? categoryId)
        {
            var vm = await _bookService.SearchAsync(title, categoryId);
            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var vm = await _bookService.GetDetailVmAsync(id);
            if (vm is null)
                return NotFound();

            return View(vm);
        }
    }
}
