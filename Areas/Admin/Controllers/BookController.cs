using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    public class BookController : AdminControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public async Task<IActionResult> Index(string? title, int? categoryId)
        {
            var vm = await _bookService.SearchAsync(title, categoryId);
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vm = await _bookService.GetDetailVmAsync(id);
            if (vm is null)
                return NotFound();

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new BookFormVM
            {
                CategoryList = await _bookService.GetCategorySelectListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.CategoryList = await _bookService.GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            var book = new Book
            {
                Title = vm.Title,
                Author = vm.Author,
                Price = vm.Price,
                Stock = vm.Stock,
                CategoryId = vm.CategoryId
            };

            await _bookService.CreateAsync(book, vm.ImageFile);

            TempData["success"] = "Book created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookService.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            var vm = new BookFormVM
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                Stock = book.Stock,
                CategoryId = book.CategoryId,
                ExistingImageUrl = book.ImageUrl,
                CategoryList = await _bookService.GetCategorySelectListAsync(book.CategoryId)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookFormVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.CategoryList = await _bookService.GetCategorySelectListAsync(vm.CategoryId);
                return View(vm);
            }

            var book = new Book
            {
                Id = vm.Id,
                Title = vm.Title,
                Author = vm.Author,
                Price = vm.Price,
                Stock = vm.Stock,
                CategoryId = vm.CategoryId
            };

            var updated = await _bookService.UpdateAsync(book, vm.ImageFile);
            if (!updated) return NotFound();

            TempData["success"] = "Book updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookService.GetByIdWithCategoryAsync(id.Value);
            if (book == null) return NotFound();

            var isReferenced = await _bookService.IsReferencedByOrdersAsync(id.Value);

            var vm = new BookDeleteVM
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                CategoryName = book.Category?.Name,
                ImageUrl = book.ImageUrl,
                IsReferencedByOrders = isReferenced
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _bookService.DeleteAsync(id);

            if (result.NotFound)
                return NotFound();

            if (!result.Success)
            {
                TempData["error"] = result.Errors.FirstOrDefault()
                    ?? "Cannot delete this book.";
                return RedirectToAction(nameof(Index));
            }

            TempData["success"] = "Book deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}