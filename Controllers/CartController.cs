using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaneProjesi.Models;
using System.Security.Claims;

namespace PastaneProjesi.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly PastaneContext _context;

        public CartController(PastaneContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var cartItems = await _context.Set<CartItem>()
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // STOCK CHECK
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return Json(new { success = false, message = "Ürün bulunamadı." });

            var cartItem = await _context.Set<CartItem>()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.Stock)
                {
                    return Json(new { success = false, message = $"Yetersiz stok! Mevcut: {product.Stock}" });
                }
                cartItem.Quantity += quantity;
                _context.Update(cartItem);
            }
            else
            {
                if (quantity > product.Stock)
                {
                    return Json(new { success = false, message = $"Yetersiz stok! Mevcut: {product.Stock}" });
                }

                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                _context.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            
            await _context.SaveChangesAsync();

            // Calculate new cart count for the badge
            var newCount = await _context.Set<CartItem>()
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
            
            return Json(new { success = true, message = "Ürün sepete eklendi.", cartCount = newCount });
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int change)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cartItem = await _context.Set<CartItem>()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null) 
                {
                    var newQuantity = cartItem.Quantity + change;

                    if (change > 0 && newQuantity > product.Stock)
                    {
                        TempData["Error"] = $"Stok yetersiz! Maksimum {product.Stock} adet alabilirsiniz.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                cartItem.Quantity += change;

                if (cartItem.Quantity <= 0)
                {
                    _context.Remove(cartItem);
                }
                else
                {
                    _context.Update(cartItem);
                }
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var cartItem = await _context.Set<CartItem>()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cartItems = await _context.Set<CartItem>()
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CartTotal = cartItems.Sum(c => c.Quantity * c.Product!.Price);
            return View(cartItems);
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string deliveryAddress, string phone, string paymentMethod, string notes)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cartItems = await _context.Set<CartItem>()
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // Final Stock Check
            foreach (var item in cartItems)
            {
                if (item.Product!.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"'{item.Product.Name}' için stok yetersiz. Lütfen sepetinizi güncelleyin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Create Order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product!.Price),
                Status = "Ödeme Bekliyor",
                DeliveryAddress = deliveryAddress,
                Phone = phone,
                Notes = notes,
                PaymentMethod = paymentMethod
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create Order Items
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    // OrderId will be set automatically by EF since we added order to context? 
                    // No, we need to add to Order.OrderItems or set OrderId after save if not using navigation prop add
                    // Better approach:
                    Product = item.Product!, // Reference
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product!.Price,
                    UserId = userId.ToString()
                };
                
                // Manually linking or adding to collection
                if(order.OrderItems == null) order.OrderItems = new List<OrderItem>();
                order.OrderItems.Add(orderItem);
                
                // Update stock? User didn't explicitly ask but it's good practice. 
                // Let's stick to basics first as requested "functionality working", can add stock management later if needed.
                // But generally checkout should reduce stock.
                item.Product!.Stock -= item.Quantity;
                _context.Update(item.Product);
            }

            // Clear Cart
            _context.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            // Simple confirmation page
            return View(id);
        }
    }
}
