using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using System.Diagnostics;

namespace PastaneProjesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PastaneContext _context;

        public HomeController(ILogger<HomeController> logger, PastaneContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Landing page - Fetch featured products (e.g., random 6 or newest)
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .OrderBy(r => Guid.NewGuid()) // Random sort
                .Take(6)
                .ToListAsync();

            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
