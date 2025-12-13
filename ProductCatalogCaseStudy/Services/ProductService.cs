using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Repositories;
using ProductCatalogCaseStudy.Repositories.Contracts;
using ProductCatalogCaseStudy.Services.Contracts;

namespace ProductCatalogCaseStudy.Services
{
    /// <summary>
    /// Implements the core business logic for the Product catalog.
    /// </summary>
    public class ProductService(IProductRepository repository, IMapper mapper) : IProductService
    {
        /// <summary>
        /// V1 logic: simply delegates to the repository to get all products.
        /// </summary>
        /// <returns>An asynchronous operation returning the products</returns>
        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await repository.GetAllAsync();
        }

        /// <summary>
        /// V2 logic: delegates to the repository's filtered/paged method.
        /// </summary>
        /// <param name="pageNumber">The page number (starts at 1)</param>
        /// <param name="pageSize">The number of elements per page</param>
        /// <returns>An asynchronous operation returning the products</returns>
        public async Task<IEnumerable<Product>> GetProducts(int pageNumber, int pageSize)
        {
            return await repository.GetAllAsync(pageNumber, pageSize);
        }

        /// <summary>
        /// Retrieves a product with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the product to retrieve.</param>
        /// <returns>An asynchronous operation returning the product</returns>
        public async Task<Product?> GetProductById(int id)
        {
            return await repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Attempts to apply a partial update to an existing product with the specified ID.
        /// </summary>
        /// <param name="id">The unique ID of the product to update.</param>
        /// <param name="productDto">An object containing the fields to update for the product. Only the provided non-null fields will be modified</param>
        /// <returns>A tuple representing the result state of the operation</returns>
        public async Task<(bool Success, bool Found)> UpdateProduct(int id, ProductPatchDto productDto)
        {

            var product = await repository.GetByIdAsync(id);

            if (product is null)
            {
                return (false, false);
            }

            try
            {
                mapper.Map(productDto, product);

                await repository.UpdateAsync(product);

                return (true, true);
            }
            catch (Exception)
            {
                return (false, true);
            }
        }
    }
}