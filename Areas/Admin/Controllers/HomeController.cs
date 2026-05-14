using BookStore.Data;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController: Controller
    {
        private const int PageSize = 10;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1) page = 1;
            
            var totalCount = await _context.Books.AsNoTracking().CountAsync();

            var totalPages = PageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)PageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new AdminHomeBooksVM
            {
                Books = books,
                CurrentPage = page,
                PageSize = PageSize,
                TotalCount = totalCount
            };

            return View(vm);

        }

    }
}
