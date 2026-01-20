using Microsoft.AspNetCore.Identity;

namespace PastaneProjesi.Models
{
    public class User : IdentityUser<int>
    {
        // Id, UserName, Email, PasswordHash are inherited from IdentityUser<int>
        
        public string FullName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsAdmin { get; set; } = false;

        // Password Reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

        // Navigation properties
        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
