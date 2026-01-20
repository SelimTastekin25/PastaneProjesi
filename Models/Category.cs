using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PastaneProjesi.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string? Name { get; set; }

        // Navigation property for related products
        public ICollection<Product>? Products { get; set; }
    }
}
