using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductCatalogCaseStudy.Controllers.V2;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using System.Text.Json;

namespace ProductCatalogCaseStudy.Tests.Controllers.V2
{
    public class ProductControllerV2Tests: IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProductController _controllerV2;

        // Total count of products seeded (25)
        private const int TotalProductCount = 25;

        public ProductControllerV2Tests()
        {
            _context = this.getDbContext();

            // Create a real AutoMapper instance

            var mapperMock = new Mock<IMapper>();
            mapperMock
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

            // Instantiate V1 Controller
            _controllerV2 = new ProductController(
                _context,
                mapperMock.Object
            );

            SeedDatabase();
        }

        private AppDbContext getDbContext(string connection = "DataSource=:memory:")
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new AppDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        }

        private void SeedDatabase()
        {
            _context.Products.RemoveRange(_context.Products);
            _context.SaveChanges();

            var jsonPath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName, "test_mock_data.json");

            // Ensure the JSON file exists (it should be copied to the output directory)
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Test seed data not found at: {jsonPath}. Ensure the file is set to 'Copy to Output Directory'.");
            }

            var jsonString = File.ReadAllText(jsonPath);

            // Deserialize the products list
            var products = JsonSerializer.Deserialize<List<Product>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Handle case differences between JSON and C#
            });

            if (products != null)
            {
                _context.Products.AddRange(products);
                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnAllProducts()
        {
            // Act
            var result = await _controllerV2.GetAllProducts(1, int.MaxValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var products = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(TotalProductCount, products.Count); // 1 out of 2 products is deactivated
        }

        [Fact]
        public async Task GetAllProducts_DefaultPagination_ReturnsFirstPageOfTen()
        {
            // Act
            var result = await _controllerV2.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var products = Assert.IsType<List<Product>>(okResult.Value);

            // Ensure the items are the first 10 products (IDs 1, 3, 5, 7, 9, 11, 13, 15, 17, 19)
            Assert.Equal(1, products[0].Id);
            Assert.Equal(10, products.Last().Id);
        }

        [Fact]
        public async Task GetAllProducts_SecondPage_ReturnsCorrectSubset()
        {
            // Act: pageNumber=2, pageSize=5
            var result = await _controllerV2.GetAllProducts(pageNumber: 2, pageSize: 5);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var products = Assert.IsType<List<Product>>(okResult.Value);

            // Page 2 (items 6 through 10) should be IDs 11, 13, 15, 17, 19 (5 items total)
            Assert.Equal(5, products.Count);

            Assert.Equal(6, products[0].Id);
            Assert.Equal(10, products.Last().Id);
        }

        [Fact]
        public async Task GetAllProducts_OutOfBoundsPage_ReturnsEmptyList()
        {
            // Act
            var result = await _controllerV2.GetAllProducts(pageNumber: 4, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var products = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Empty(products);
        }

        [Fact]
        public async Task GetProductById_ProductExists_ReturnsProduct()
        {
            // Arrange
            int existentId = 9;

            // Act
            var result = await _controllerV2.GetProductById(existentId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(9, product.Id);
            Assert.Equal("Adjustable Desk Riser", product.Name);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _controllerV2.GetProductById(nonExistentId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateAllProperties()
        {
            // Arrange
            int productId = 3; // Use a different product ID for isolation
            var originalProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId);

            var newName = "Fully Updated Product";
            var newDescription = "New Description for V1 Patch Test";
            var newImgUri = "/images/full_update.jpg"; // New field
            var newPrice = 55.55m;
            var newCurrency = "USD"; // New field
            var newStock = 200;

            var patchDto = new ProductPatchDto
            {
                Name = newName,
                Description = newDescription,
                ImgUri = newImgUri,
                Price = newPrice,
                Currency = newCurrency,
                StockQuantity = newStock
            };

            // Act
            var result = await _controllerV2.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedProduct = _context.Products.Find(productId);

            // Check that ALL properties were successfully updated
            Assert.Equal(newName, updatedProduct!.Name);
            Assert.Equal(newDescription, updatedProduct.Description);
            Assert.Equal(newImgUri, updatedProduct.ImgUri); // Assert new field
            Assert.Equal(newPrice, updatedProduct.Price);
            Assert.Equal(newCurrency, updatedProduct.Currency); // Assert new field
            Assert.Equal(newStock, updatedProduct.StockQuantity);

            // Check that original properties are different from the new ones
            Assert.NotEqual(originalProduct!.Name, updatedProduct.Name);
            Assert.NotEqual(originalProduct.Price, updatedProduct.Price);
        }

        [Fact]
        public async Task UpdateProduct_ShouldApplyNonNullPropertiesAndIgnoreNulls()
        {
            int productId = 1;
            var originalProduct = _context.Products.Find(productId);

            // Arrange
            var patchDto = new ProductPatchDto
            {
                Name = "Updated Name", // Non-null: Should change
                Description = null,    // Null: Should be ignored
                ImgUri = "/images/new_img.png", // Non-null: Should change
                Price = 99.99m,        // Non-null: Should change
                Currency = null,       // Null: Should be ignored
                StockQuantity = null   // Null: Should be ignored
            };

            // Act
            var result = await _controllerV2.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedProduct = _context.Products.Find(productId);
            Assert.Equal("Updated Name", updatedProduct!.Name);
            Assert.Equal("/images/new_img.png", updatedProduct.ImgUri); // New field check
            Assert.Equal(99.99m, updatedProduct.Price);

            // Assert Description, Currency, and StockQuantity were not changed (ignored due to null DTO values)
            Assert.Equal(originalProduct!.Description, updatedProduct.Description);
            Assert.Equal(originalProduct.Currency, updatedProduct.Currency); // New field check
            Assert.Equal(originalProduct.StockQuantity, updatedProduct.StockQuantity);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNotFoundForInvalidId()
        {
            // Arrange
            int nonExistentId = 999;
            var patchDto = new ProductPatchDto { Name = "Test" };

            // Act
            var result = await _controllerV2.UpdateProduct(nonExistentId, patchDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequestOnInvalidProperty()
        {
            // Arrange
            int productId = 1;
            var patchDto = new ProductPatchDto { }; // Invalid price

            // Act
            _controllerV2.ModelState.AddModelError(nameof(patchDto.Price), "Price must be greater than zero.");
            var result = await _controllerV2.UpdateProduct(productId, patchDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // --- Cleanup ---
        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}