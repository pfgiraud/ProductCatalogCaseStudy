using System.ComponentModel.DataAnnotations;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.DTO
{
    /// <summary>
    /// Represents the fields that can be updated during a PATCH operation on a Product.
    /// Only include fields here that are allowed for modification via a patch request.
    /// </summary>
    public class ProductPatchDto
    {
        /// <summary>
        /// The updated name of the product.
        /// </summary>
        [MaxLength(255)]
        public string? Name { get; set; }

        /// <summary>
        /// A brief description of the product.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The relative URL to the image representing the product
        /// </summary>
        public string? ImgUri { get; set; }

        /// <summary>
        /// The new price of the product.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// The currency used for the price.
        /// </summary>
        [MaxLength(3)]
        public string? Currency { get; set; }

        /// <summary>
        /// The updated number of units in stock.
        /// </summary>
        public int? StockQuantity { get; set; }
    }
}
