using BookStore.Data;
using BookStore.Models;
using BookStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    public class OrderController : AdminControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllForAdminAsync();
            return View(orders);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null)
                return NotFound();

            var order = await _orderService.GetDetailForAdminAsync(id.Value);
            if (order is null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!OrderStatuses.All.Contains(status))
            {
                TempData["error"] = "Invalid status.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var updated = await _orderService.UpdateStatusAsync(id, status);
            if (!updated)
                return NotFound();

            TempData["success"] = "Order status updated.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
