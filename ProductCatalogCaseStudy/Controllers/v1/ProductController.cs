using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Services.Contracts;

namespace ProductCatalogCaseStudy.Controllers.V1
{
    /// <summary>
    /// The product catalog
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]s")]
    [ApiVersion("1", Deprecated = true)]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {
        // READ ALL
        /// <summary>
        /// Retrieves a list of all products in the catalog.
        /// </summary>
        /// <returns>A list of product objects</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var products = await productService.GetProducts();

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
            var product = await productService.GetProductById(id);

            if (product == null)
            {
                return NotFound("Product ID not found");
            }

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
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductPatchDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (productDto == null)
            {
                return BadRequest("Request body cannot be empty.");
            }

            var (success, found)  = await productService.UpdateProduct(id, productDto);
            // Check if the ID in the URL matches the ID in the product body
            if (!found)
            {
                return NotFound("Product ID not found");
            }

            if (!success)
            {
                return Problem("Update failed due to an unknown service error.", statusCode: StatusCodes.Status500InternalServerError);
            }

            // Standard response for a successful PATCH request
            return NoContent();
        }
    }
}