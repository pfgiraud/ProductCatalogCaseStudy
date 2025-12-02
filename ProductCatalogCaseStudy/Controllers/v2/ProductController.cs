using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
namespace ProductCatalogCaseStudy.Controllers.V2
{
    /// <summary>
    /// The product catalog
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]s")]
    [ApiVersion("2")]
    [ApiExplorerSettings(GroupName = "v2")]
    [ApiController]
    public class ProductController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        // READ ALL
        /// <summary>
        /// Retrieves a list of all products in the catalog.
        /// </summary>
        /// <remarks>
        /// V2 returns only active products and supports pagination.
        /// </remarks>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>A list of product objects</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            // Ensure valid pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 1;

            var products = await _context.Products
                .Where(p => !p.IsDeactived)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Simple query to fetch all products from the database
            return Ok(products);
        }

        // READ ONE
        /// <summary>
        /// Retrieves a single existing product
        /// </summary>
        /// <param name="id">The unique identifier of the product</param>
        /// <returns>The product data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            // Find the product by its primary key
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                // Returns a 404 Not Found response
                return NotFound("Product ID not found");
            }

            // Returns a 200 OK response with the product object
            return Ok(product);
        }

        // PARTIAL EDIT
        /// <summary>
        /// Updates partially an existing product
        /// </summary>
        /// <remarks>
        /// This method requires a JSON Patch document (RFC 6902) in the request body. 
        /// Use 'application/json-patch+json' content type.
        /// </remarks>
        /// <param name="id">The unique identifier of the product to update</param>
        /// <param name="productDto">The updated product object</param>
        /// <returns>A status code indicating the result of the update</returns>
        [HttpPatch("{id}")]
        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] int id,
            [FromBody] ProductPatchDto productDto
            )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            // Check if the ID in the URL matches the ID in the product body
            if (product is null)
            {
                return NotFound("Product ID not found");
            }

            if (productDto == null)
            {
                return BadRequest("Request body cannot be empty.");
            }

            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Standard response for a successful PATCH request
            return NoContent();
        }
    }
}