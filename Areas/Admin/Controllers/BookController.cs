using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Book
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
            return View(books);
        }

        // GET: Admin/Book/Create
        public async Task<IActionResult> Create()
        {
            var vm = new BookFormVM
            {
                CategoryList = await GetCategoryListAsync()
            };
            return View(vm);
        }

        // POST: Admin/Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.CategoryList = await GetCategoryListAsync(vm.CategoryId);
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

            if (vm.ImageFile != null)
            {
                book.ImageUrl = await SaveImageAsync(vm.ImageFile);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["success"] = "Book created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Book/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
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
                CategoryList = await GetCategoryListAsync(book.CategoryId)
            };

            return View(vm);
        }

        // POST: Admin/Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookFormVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.CategoryList = await GetCategoryListAsync(vm.CategoryId);
                return View(vm);
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            book.Title = vm.Title;
            book.Author = vm.Author;
            book.Price = vm.Price;
            book.Stock = vm.Stock;
            book.CategoryId = vm.CategoryId;

            if (vm.ImageFile != null)
            {
                // Delete old image first if exists
                if (!string.IsNullOrWhiteSpace(book.ImageUrl))
                {
                    DeleteImageFile(book.ImageUrl);
                }
                book.ImageUrl = await SaveImageAsync(vm.ImageFile);
            }
            await _context.SaveChangesAsync();
            TempData["success"] = "Book updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Book/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Admin/Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(book.ImageUrl))
            {
                DeleteImageFile(book.ImageUrl);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["success"] = "Book deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // === PRIVATE HELPER ===
        private async Task<IEnumerable<SelectListItem>> GetCategoryListAsync(int? selectedId = null)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                })
                .ToListAsync();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "books");
            Directory.CreateDirectory(uploadsFolder); // auto create if not exists
            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return $"/images/books/{fileName}";
        }

        private void DeleteImageFile(string imageUrl)
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
