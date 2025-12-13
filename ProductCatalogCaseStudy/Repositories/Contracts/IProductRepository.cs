using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.Repositories.Contracts
{
    /// <summary>
    /// Provides methods to access and manage <see cref="Product"/> entities in the repository.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>The product with the specified identifier, or <c>null</c> if not found.</returns>
        public Task<Product?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all products from the repository.
        /// </summary>
        /// <returns>An enumerable collection of all products.</returns>
        public Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// Retrieves products paginated from the repository.
        /// </summary>
        /// <returns>An enumerable collection of all products.</returns>
        public Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Updates the specified product in the repository.
        /// </summary>
        /// <param name="product">The product to update.</param>
        public Task UpdateAsync(Product product);
    }
}