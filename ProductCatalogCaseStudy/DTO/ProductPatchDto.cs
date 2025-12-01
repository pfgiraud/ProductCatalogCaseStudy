using System.ComponentModel.DataAnnotations;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.DTO
{
    public class ProductPatchDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImgUri { get; set; }
        public decimal? Price { get; set; }

        [MaxLength(3)]
        public string? Currency { get; set; }

        public int? StockQuantity { get; set; }
    }
}
