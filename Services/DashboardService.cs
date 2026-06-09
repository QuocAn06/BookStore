using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class DashboardService: IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminHomeBooksVM> GetDashboardAsync(int page, int pageSize = 10)
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

            var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .OrderByDescending(b => b.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AdminHomeBooksVM
            {
                Books = books,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue
            };
        }
    }
}
