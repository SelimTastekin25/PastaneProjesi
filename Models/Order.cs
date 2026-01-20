namespace PastaneProjesi.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public decimal TotalAmount { get; set; }
        
        public string Status { get; set; } = "Ödeme Bekliyor"; // Ödeme Bekliyor, Hazırlanıyor, Teslim Edildi
        
        public string DeliveryAddress { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string? Notes { get; set; }
        
        public string PaymentMethod { get; set; } = "Kapıda Ödeme"; // Kredi Kartı, Havale/EFT, Kapıda Ödeme

        // Navigation properties
        public User? User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
