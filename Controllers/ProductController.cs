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
        [HttpGet]
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
        [HttpGet("{id}")]
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
        /// <param name="id">The unique identifier of the product to update</param>
        /// <param name="product">The updated product object</param>
        [HttpPatch("{id}")]
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