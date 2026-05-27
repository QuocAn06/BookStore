using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    public class OrderController : AdminControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Admin/Order/Detail/5
        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
                return NotFound();

            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!OrderStatuses.All.Contains(status))
            {
                TempData["error"] = "Invalid status.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var order = await _context.Orders.FindAsync(id);
            if (order is null)
                return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["success"] = "Order status updated.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
