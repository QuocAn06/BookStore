using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    public class HomeController: AdminControllerBase
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

            var totalOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync();

            var totalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatuses.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

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
                TotalCount = totalCount,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };

            return View(vm);

        }

    }
}
