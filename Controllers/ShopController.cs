using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using PastaneProjesi.Models.ViewModels;

namespace PastaneProjesi.Controllers
{
    public class ShopController : Controller
    {
        private readonly PastaneContext _context;

        public ShopController(PastaneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int? categoryId, string? categoryName, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // 1. Text Search
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => 
                    (p.Name != null && p.Name.Contains(search)) || 
                    (p.Description != null && p.Description.Contains(search)));
            }

            // 2. Category Filter
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            else if (!string.IsNullOrEmpty(categoryName))
            {
                productsQuery = productsQuery.Where(p => p.Category!.Name == categoryName);
                ViewData["CurrentCategoryName"] = categoryName; // For UI highlight if needed
            }

            // 3. Price Filter
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. Sorting
            productsQuery = sortOrder switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "name_asc" => productsQuery.OrderBy(p => p.Name),
                "name_desc" => productsQuery.OrderByDescending(p => p.Name),
                _ => productsQuery.OrderBy(p => p.Name) // Default: A-Z
            };

            var viewModel = new ShopViewModel
            {
                Products = await productsQuery.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                CurrentSearch = search,
                CurrentCategory = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}
