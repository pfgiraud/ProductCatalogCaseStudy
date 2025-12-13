using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.Services.Contracts
{
    /// <summary>
    /// Defines the contract for product-related business logic and orchestration.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Asynchronously retrieves all available products.
        /// </summary>
        /// <returns>An asynchronous operation returning the products</returns>
        Task<IEnumerable<Product>> GetProducts();

        /// <summary>
        /// Asynchronously retrieves a paginated collection of products.
        /// </summary>
        /// <param name="pageNumber">The 1-based index of the page to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The maximum number of products to include in the returned page. Must be greater than 0.</param>
        /// <returns>An asynchronous operation returning the products</returns>
        Task<IEnumerable<Product>> GetProducts(int pageNumber, int pageSize);

        /// <summary>
        /// Asynchronously retrieves a product with the specified ID.
        /// </summary>
        /// <param name="id">The unique IDof the product to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product with the specified
        /// identifier, or <see langword="null"/> if no matching product is found.</returns>
        // Simple peration to fetch a product by its ID
        Task<Product?> GetProductById(int id);

        /// <summary>
        /// Asynchronously update a product details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDto">The changes made (only non null values will be updated)</param>
        /// <returns></returns>
        // 
        Task<(bool Success, bool Found)> UpdateProduct(int id, ProductPatchDto patchDto);
    }
}
