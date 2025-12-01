using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogCaseStudy.Controllers
{
    /// <summary>
    /// The product catalog
    /// </summary>
    [Route("api/[controller]s")]
    [ApiController]
    public class ProductController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        // READ ALL
        /// <summary>
        /// Retrieves a list of all products in the catalog.
        /// </summary>
        /// <returns>A list of product objects</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            // Simple query to fetch all products from the database
            return await _context.Products.ToListAsync();
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
        /// <param name="patchDocument">The updated product object</param>
        /// <returns>A status code indicating the result of the update</returns>
        [HttpPatch("{id}")]
        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] JsonPatchDocument<ProductPatchDto> patchDocument)
        {
            var product = await _context.Products.FindAsync(id);
            // Check if the ID in the URL matches the ID in the product body
            if (product is null)
            {
                return NotFound("Product ID not found");
            }

            // Apply the patch (Get DTO from product, apply JSON to DTO, then apply DTO back to product)
            var productDto = _mapper.Map<ProductPatchDto>(product);
            patchDocument.ApplyTo(productDto);
            _mapper.Map(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Standard response for a successful PATCH request
            return NoContent();
        }
    }
}