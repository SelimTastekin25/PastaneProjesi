using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using System.Security.Claims;

namespace PastaneProjesi.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly PastaneContext _context;

        public CartSummaryViewComponent(PastaneContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View(0);
            }

            var userId = int.Parse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var itemCount = await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            return View(itemCount);
        }
    }
}
