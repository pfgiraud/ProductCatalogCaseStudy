using AutoMapper;
using Moq;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Repositories.Contracts;
using ProductCatalogCaseStudy.Services;
using System.Text.Json;

namespace ProductCatalogCaseStudy.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductService _service;
        private readonly List<Product> _sampleProducts;

        public ProductServiceTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockMapper
                .Setup(m => m.Map(It.IsAny<ProductPatchDto>(), It.IsAny<Product>()))
                .Callback<ProductPatchDto, Product>((src, dest) =>
                {
                    if (src.Name is not null) dest.Name = src.Name;
                    if (src.Description is not null) dest.Description = src.Description;
                    if (src.ImgUri is not null) dest.ImgUri = src.ImgUri;
                    if (src.Price.HasValue) dest.Price = src.Price.Value;
                    if (src.Currency is not null) dest.Currency = src.Currency;
                    if (src.StockQuantity.HasValue) dest.StockQuantity = src.StockQuantity.Value;
                });
            _service = new ProductService(_mockRepository.Object, _mockMapper.Object);
            _sampleProducts = LoadSampleProducts();
        }

        private static List<Product> LoadSampleProducts()
        {
            var jsonPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName, "test_mock_data.json");

            // Ensure the JSON file exists (it should be copied to the output directory)
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Test seed data not found at: {jsonPath}. Ensure the file is set to 'Copy to Output Directory'.");
            }

            var jsonString = File.ReadAllText(jsonPath);

            // Deserialize the products list
            return JsonSerializer.Deserialize<List<Product>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Handle case differences between JSON and C#
            }) ?? [];
        }

        [Fact]
        public async Task GetProductsAsync_ShouldReturnAllProductsFromRepository()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(_sampleProducts);

            // Act
            var result = await _service.GetProducts();

            // Assert
            Assert.Equal(_sampleProducts.Count, result.Count());
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductsAsync_Paginated_ShouldCallRepositoryWithCorrectPagination()
        {
            // Arrange
            int pageNumber = 2;
            int pageSize = 5;
            _mockRepository.Setup(r => r.GetAllAsync(pageNumber, pageSize))
                           .ReturnsAsync(_sampleProducts.Skip(pageSize).Take(pageSize));

            // Act
            var products = await _service.GetProducts(pageNumber, pageSize);

            // Assert
            Assert.Equal(pageSize, products.Count());
            _mockRepository.Verify(r => r.GetAllAsync(pageNumber, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenFound()
        {
            // Arrange
            int productId = 1;
            var expectedProduct = _sampleProducts.First();
            _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // Act
            var product = await _service.GetProductById(productId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
            _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            int productId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var product = await _service.GetProductById(productId);

            // Assert
            Assert.Null(product);
            _mockRepository.Verify(r => r.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;
            var patchDto = new ProductPatchDto { Name = "New Name" };

            // Repository returns null (Not Found)
            _mockRepository.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Product?)null);

            // Act
            var (success, found) = await _service.UpdateProduct(nonExistentId, patchDto);

            // Assert
            Assert.False(success);
            Assert.False(found);

            // Should stop before mapping or updating
            _mockMapper.Verify(m => m.Map(It.IsAny<ProductPatchDto>(), It.IsAny<Product>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldMapAndCallRepositoryUpdate_OnSuccess()
        {
            // Arrange
            int productId = 1;
            var existingProduct = _sampleProducts.First();
            var patchDto = new ProductPatchDto { Price = 125.99m };
            var updatedAtDateBefore = existingProduct.UpdatedAt;

            _mockRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            // Act
            var (success, found) = await _service.UpdateProduct(productId, patchDto);

            // Assert
            Assert.True(success);
            Assert.True(found);
            Assert.NotEqual(updatedAtDateBefore, existingProduct.UpdatedAt);

            _mockMapper.Verify(m => m.Map(patchDto, existingProduct), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        }
    }
}
