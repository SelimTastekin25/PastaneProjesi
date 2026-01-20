using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PastaneProjesi.Models
{
    public class PastaneContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PastaneContext(DbContextOptions<PastaneContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; } = default!;
    }
}