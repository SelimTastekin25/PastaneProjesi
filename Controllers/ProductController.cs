using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using ClosedXML.Excel;
using System.IO;

namespace PastaneProjesi.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly PastaneContext _context;

        public ProductController(PastaneContext context)
        {
            _context = context;
        }

        // GET: Product/Dashboard
        public async Task<IActionResult> Dashboard(string searchString, int? categoryId)
        {
            // Stats for Cards
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.CriticalStock = await _context.Products.CountAsync(p => p.Stock < 5);
            
            if (await _context.Products.AnyAsync())
            {
                ViewBag.AveragePrice = await _context.Products.AverageAsync(p => p.Price);
            }
            else
            {
                ViewBag.AveragePrice = 0;
            }

            // Pass all categories to view for filter buttons
            ViewBag.Categories = await _context.Categories.ToListAsync();
            
            // Pass current filter values back to view
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;

            // Start with queryable collection
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => 
                    (p.Name != null && p.Name.Contains(searchString)) || 
                    (p.Description != null && p.Description.Contains(searchString)));
            }

            // Apply category filter if categoryId is provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return View(await products.ToListAsync());
        }

        // GET: Product
        // Admin Panel: List all products for management with search and filter
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // Dashboard Stats
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.CriticalStock = await _context.Products.Where(p => p.Stock < 5).CountAsync();
            
            if (await _context.Products.AnyAsync())
            {
                ViewBag.AveragePrice = await _context.Products.AverageAsync(p => p.Price);
            }
            else
            {
                ViewBag.AveragePrice = 0;
            }

            // Pass all categories to view for filter buttons
            ViewBag.Categories = await _context.Categories.ToListAsync();
            
            // Pass current filter values back to view
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;

            // Start with queryable collection
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply search filter if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => 
    (p.Name != null && p.Name.Contains(searchString)) || 
    (p.Description != null && p.Description.Contains(searchString)));
            }

            // Apply category filter if categoryId is provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return View(await products.ToListAsync());
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Stock,CategoryId,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            try
            {
                _context.Update(product); 
                await _context.SaveChangesAsync(); 
                
                TempData["SuccessMessage"] = "Stok başarıyla güncellendi!";
                return RedirectToAction("Dashboard"); 
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu.");
                return View(product);
            }
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Urunler");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "Ürün Adı";
                worksheet.Cell(currentRow, 2).Value = "Kategori";
                worksheet.Cell(currentRow, 3).Value = "Fiyat";
                worksheet.Cell(currentRow, 4).Value = "Stok";

                // Styling Headers
                worksheet.Range("A1:D1").Style.Font.Bold = true;
                worksheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                foreach (var product in products)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = product.Name;
                    worksheet.Cell(currentRow, 2).Value = product.Category?.Name ?? "-";
                    worksheet.Cell(currentRow, 3).Value = product.Price;
                    worksheet.Cell(currentRow, 4).Value = product.Stock;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pastane_Urun_Raporu.xlsx");
                }
            }
        }
    }
}
