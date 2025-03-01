using System.ComponentModel.DataAnnotations;

namespace bicycleBackend.Model
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string ImageUrl { get; set; } // ✅ Sadrži URL slike proizvoda

        [Required]
        public string Category { get; set; }
    }
}
