using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductCatalogCaseStudy.Models
{
    /// <summary>
    /// Represents a single product entity in the catalog.
    /// </summary>
    public class Product
    {
        // --- Core Information ---

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public required string ImgUri { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Sku { get; set; } // Stock Keeping Unit

        [MaxLength(255)]
        public string? Slug { get; set; } // URL-friendly name

        // --- Pricing & Availability ---

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal Price { get; set; }

        [Required]
        [MaxLength(3)]
        public required string Currency { get; set; } = "CZK";

        public int StockQuantity { get; set; } = 0;

        public bool IsInStock => StockQuantity > 0;

        // --- Status Tracking ---

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeactived { get; set; } = false;
    }
}
