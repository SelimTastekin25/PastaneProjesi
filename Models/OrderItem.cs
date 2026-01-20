namespace PastaneProjesi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        // Eğer kullanıcı bağlantısı eklendiyse:
        public string? UserId { get; set; } 
    }
}