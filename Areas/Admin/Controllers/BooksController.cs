using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    public class BooksController: AdminControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? title, int? categoryId)
        {
            IQueryable<Book> query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category);

            if (!string.IsNullOrWhiteSpace(title))
            {
                var term = title.Trim();
                query = query.Where(b => b.Title.Contains(term));
            }

            if (categoryId is int cid && cid > 0)
            {
                query = query.Where(b => b.CategoryId == cid);
            }

            var books = await query
                .OrderBy(b => b.Title)
                .ToListAsync();

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = categoryId == c.Id
                })
                .ToListAsync();

            categories.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "All categories",
                Selected = categoryId is null or 0
            });

            var vm = new AdminBooksIndexVM
            {
                Books = books,
                TitleSearch = title,
                CategoryId = categoryId,
                CategoryOptions = categories
            };

            return View(vm);
        }

        // GET: Admin/Books/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            // Load the main book, including related data
            var book = await _context.Books
                .Include(b => b.Category)   // related Category
                // .Include(b => b.Publisher) // add more Includes as needed
                // .Include(b => b.BookTags).ThenInclude(bt => bt.Tag) // example of nested relation
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            // Fetch other books in the same category (as an example of related data)
            List<Book>? relatedBooksInCategory = null;
            if (book.CategoryId != 0)
            {
                relatedBooksInCategory = await _context.Books
                    .Where(b => b.CategoryId == book.CategoryId && b.Id != book.Id)
                    .OrderBy(b => b.Title)
                    .Take(5)
                    .ToListAsync();
            }

            var vm = new BookDetailVM
            {
                Book = book,
                RelatedBooksInCategory = relatedBooksInCategory
            };
            return View(vm);
        }

    }
}
