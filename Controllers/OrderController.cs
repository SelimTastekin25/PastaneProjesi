using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using System.Security.Claims;

namespace PastaneProjesi.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly PastaneContext _context;

        public OrderController(PastaneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.User) // Include User info for Admin view
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Security Check: Allow if Admin OR if Order belongs to current user
            if (!User.IsInRole("Admin") && order.UserId != int.Parse(userId))
            {
                return RedirectToAction("AccessDenied", "Account"); // Or NotFound()
            }

            return View(order);
        }
        // Admin: Manage All Orders
        public async Task<IActionResult> Manage()
        {
            if (!User.IsInRole("Admin")) return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Admin: Update Order Status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? returnUrl)
        {
            if (!User.IsInRole("Admin")) return RedirectToAction("Index", "Home");

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Sipariş #{id} durumu güncellendi: {status}";

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl == "Details")
            {
                return RedirectToAction("Details", new { id = id });
            }

            return RedirectToAction("Manage");
        }
    }
}
