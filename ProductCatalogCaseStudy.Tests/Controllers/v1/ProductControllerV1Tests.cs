using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductCatalogCaseStudy.Controllers.V1;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Services.Contracts;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProductCatalogCaseStudy.Tests.Controllers.V1
{
    public class ProductControllerV1Tests
    {
        private readonly ProductController _controllerV1;
        private readonly Mock<IProductService> _mockService;
        private readonly List<Product> _sampleProducts;

        public ProductControllerV1Tests()
        {
            _mockService = new Mock<IProductService>();
            _controllerV1 = new ProductController(_mockService.Object);
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
        public async Task GetAllProducts_ShouldReturnAllProducts()
        {
            _mockService.Setup(s => s.GetProducts())
                        .ReturnsAsync(_sampleProducts.ToList());
            // Act
            var result = (await _controllerV1.GetAllProducts()).Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var products = Assert.IsType<List<Product>>(result.Value);
            Assert.Equal(_sampleProducts.Count, products.Count);
            _mockService.Verify(s => s.GetProducts(), Times.Once);
        }

        [Fact]
        public async Task GetProductById_ProductExists_ReturnsProduct()
        {
            // Arrange
            const int existingId = 9;
            var expectedProduct = _sampleProducts.First(p => p.Id == existingId);
            _mockService.Setup(s => s.GetProductById(existingId))
                        .ReturnsAsync(expectedProduct);

            // Act
            var result = (await _controllerV1.GetProductById(existingId)).Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var product = Assert.IsType<Product>(result.Value);
            Assert.Equal(existingId, product.Id);
            Assert.Equal("Adjustable Desk Riser", product.Name);
            _mockService.Verify(s => s.GetProductById(existingId), Times.Once);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            const int nonExistingId = 999;

            // Act
            var result = await _controllerV1.GetProductById(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NotFoundObjectResult>(result.Result);
            _mockService.Verify(s => s.GetProductById(nonExistingId), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateAllProperties()
        {
            // Arrange
            const int productId = 3;            
            var patchDto = new ProductPatchDto
            {
                Name = "Updated Product"
            };
            _mockService.Setup(s => s.UpdateProduct(productId, It.IsAny<ProductPatchDto>()))
                        .ReturnsAsync((true, true));

            // Act
            var result = await _controllerV1.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.UpdateProduct(productId, It.IsAny<ProductPatchDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            int nonExistentId = 999;
            var patchDto = new ProductPatchDto { Name = "Test" };
            _mockService.Setup(s => s.UpdateProduct(nonExistentId, It.IsAny<ProductPatchDto>()))
                        .ReturnsAsync((false, false));

            // Act
            var result = await _controllerV1.UpdateProduct(nonExistentId, patchDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mockService.Verify(s => s.UpdateProduct(nonExistentId, It.IsAny<ProductPatchDto>()), Times.Once);
        }

        [Fact]
        public async Task PartiallyUpdateProduct_ShouldReturnBadRequestOnServiceFailure()
        {
            // Arrange
            int productId = 1;
            var patchDto = new ProductPatchDto { Name = "Test" };
            _mockService.Setup(s => s.UpdateProduct(productId, It.IsAny<ProductPatchDto>()))
                        .ReturnsAsync((false, true));

            // Act
            var result = await _controllerV1.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ((ObjectResult)result).StatusCode);
            _mockService.Verify(s => s.UpdateProduct(productId, It.IsAny<ProductPatchDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequestOnInvalidProperty()
        {
            // Arrange
            int productId = 1;
            var patchDto = new ProductPatchDto { Price = 0 }; // Invalid price

            // Act
            _controllerV1.ModelState.AddModelError(nameof(patchDto.Price), "Price must be greater than zero.");
            var result = await _controllerV1.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockService.Verify(s => s.UpdateProduct(It.IsAny<int>(), It.IsAny<ProductPatchDto>()), Times.Never);
        }
    }
}