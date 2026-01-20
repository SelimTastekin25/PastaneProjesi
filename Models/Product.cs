using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastaneProjesi.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [StringLength(150, ErrorMessage = "Ürün adı en fazla 150 karakter olabilir.")]
        [Display(Name = "Ürün Adı")]
        public string? Name { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        [Range(0.01, 10000, ErrorMessage = "Fiyat 0.01 ile 10000 arasında olmalıdır.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok adedi gereklidir.")]
        [Range(0, 10000, ErrorMessage = "Stok 0 ile 10000 arasında olmalıdır.")]
        [Display(Name = "Stok")]
        public int Stock { get; set; }

        [Display(Name = "Resim URL")]
        public string? ImageUrl { get; set; }

        // Foreign Key
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // Navigation Property
        public Category? Category { get; set; }
    }
}
