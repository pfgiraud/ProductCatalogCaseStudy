using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Repositories.Contracts;

namespace ProductCatalogCaseStudy.Repositories
{
    /// <summary>
    /// Implements IProductRepository, hiding EF Core details from the service layer.
    /// </summary>
    public class ProductRepository(AppDbContext context) : IProductRepository, IDisposable
    {
        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>The product with the specified identifier, or <c>null</c> if not found.</returns>
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await context.Products.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all products from the repository.
        /// </summary>
        /// <returns>An enumerable collection of all products.</returns>
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await context.Products.ToListAsync();
        }

        /// <summary>
        /// Retrieves products paginated from the repository.
        /// </summary>
        /// <returns>An enumerable collection of all products.</returns>
        public async Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await context.Products
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Updates the specified product in the repository.
        /// </summary>
        /// <param name="product">The product to update.</param>
        public Task UpdateAsync(Product product)
        {
            return context.SaveChangesAsync();
        }
        private bool disposed = false;

        /// <summary>
        /// Releases the unmanaged resources used by the object and, optionally, releases the managed resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}