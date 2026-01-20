namespace PastaneProjesi.Models.ViewModels
{
    public class ShopViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        
        // Filter States
        public string? CurrentSearch { get; set; }
        public int? CurrentCategory { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortOrder { get; set; }
    }
}
