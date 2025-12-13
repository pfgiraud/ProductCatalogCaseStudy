using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ProductCatalogCaseStudy.Tests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProductRepository _repository;

        // Total count of products seeded (25)
        private const int TotalProductCount = 25;

        public ProductRepositoryTests()
        {
            _context = this.getDbContext("DataSource=:memory:");
            _repository = new ProductRepository(_context);

            SeedDatabase();
        }

        private AppDbContext getDbContext(string connection)
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
        public async Task GetAllAsync_ShouldReturnAllProducts()
        {
            // Act
            var products = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(TotalProductCount, products.Count());
        }

        [Fact]
        public async Task GetAllAsync_DefaultPagination_ReturnsFirstPageOfTen()
        {
            //Arrange
            var pageNumber = 1;
            var pageSize = 10;

            // Act
            var products = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.Equal(pageSize, products.Count());
            Assert.Equal(1, products.First().Id);
            Assert.Equal(10, products.Last().Id);
        }

        [Fact]
        public async Task GetAllAsync_SecondPage_ReturnsCorrectSubset()
        {
            //Arrange
            var pageNumber = 2;
            var pageSize = 5;

            // Act
            var products = await _repository.GetAllAsync(pageNumber, pageSize);

            // Assert
            Assert.Equal(pageSize, products.Count());
            Assert.Equal(6, products.First().Id);
            Assert.Equal(10, products.Last().Id);
        }

        [Fact]
        public async Task GetAllAsync_OutOfBoundsPage_ReturnsEmptyList()
        {
            // Act
            var products = await _repository.GetAllAsync(pageNumber: 5, pageSize: 10);

            // Assert
            Assert.Empty(products);
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductExists_ReturnsProduct()
        {
            // Arrange
            int existentId = 9;

            // Act
            var product = await _repository.GetByIdAsync(existentId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(9, product.Id);
            Assert.Equal("Adjustable Desk Riser", product.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNullForInvalidId()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var product = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(product);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertProducts()
        {
            // Arrange
            var product = new Product
            {
                Name = "New product",
                Description = "New description",
                ImgUri = "New description",
                Sku = "New description",
                Price = 99,
                Currency = "USD",
            };

            // Act
            await _repository.InsertAsync([product]);

            // Assert
            var products = await _repository.GetAllAsync();
            Assert.Equal(product.Id, products.Max(p => p.Id));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllProperties()
        {
            // Arrange
            int productId = 3; // Use a different product ID for isolation
            var originalProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId);
            var product = (await _repository.GetByIdAsync(productId))!;

            var newName = "Fully Updated Product";
            var newDescription = "New Description for V1 Patch Test";
            var newImgUri = "/images/full_update.jpg"; // New field
            var newPrice = 55.55m;
            var newCurrency = "USD"; // New field
            var newStock = 200;

            product.Name = newName;
            product.Description = newDescription;
            product.ImgUri = newImgUri;
            product.Price = newPrice;
            product.Currency = newCurrency;
            product.StockQuantity = newStock;

            // Act
            await _repository.UpdateAsync(product);
            var updatedProduct = await _repository.GetByIdAsync(productId);

            // Check that ALL properties were successfully updated
            Assert.Equal(newName, updatedProduct!.Name);
            Assert.Equal(newDescription, updatedProduct.Description);
            Assert.Equal(newImgUri, updatedProduct.ImgUri);
            Assert.Equal(newPrice, updatedProduct.Price);
            Assert.Equal(newCurrency, updatedProduct.Currency);
            Assert.Equal(newStock, updatedProduct.StockQuantity);

            // Check that original properties are different from the new ones
            Assert.NotEqual(originalProduct!.Name, updatedProduct.Name);
            Assert.NotEqual(originalProduct.Price, updatedProduct.Price);
        }

        // --- Cleanup ---
        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}