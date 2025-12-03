using ProductCatalogCaseStudy.Controllers;
using ProductCatalogCaseStudy.Models;
using System.Text.Json;

namespace ProductCatalogCaseStudy.Data
{
    /// <summary>
    /// This static class handles reading the external JSON seed data and inserting it in the database.
    /// </summary>
    public static class ProductSeeder
    {

        /// <summary>
        /// Read the external JSON seed data and insert it in the database  
        /// </summary>
        public static bool Initialize(AppDbContext context, string jsonFilePath, ILogger logger)
        {
            // Check if the database already has been seeded.
            if (context.Products.Any())
            {
                return false;
            }

            if (!File.Exists(jsonFilePath))
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                {
                    logger.Log(
                        LogLevel.Error,
                        new EventId(0, "SeedingError"),
                        $"Seed data file not found at : {jsonFilePath}",
                        null,
                        (state, ex) => state.ToString());
                }
                return false;
            }

            // Read and deserialize the JSON data.
            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                var products = JsonSerializer.Deserialize<List<Product>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Ensures matching regardless of casing
                });

                if (products != null && products.Any())
                {
                    // Reset the PK identity for safe insertion (the DB should handle the IDs itself not the JSON)
                    foreach (var item in products)
                    {
                        item.Id = 0;
                    }

                    context.Products.AddRange(products);
                    context.SaveChanges();
                    if (logger?.IsEnabled(LogLevel.Debug) == true)
                    {
                        logger.Log(
                            LogLevel.Debug,
                            new EventId(0, "SeedingSuccess"),
                            $"Successfully seeded {products.Count} products from JSON : {jsonFilePath}",
                            null,
                            (state, ex) => state.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                {
                    logger.Log(
                        LogLevel.Error,
                        new EventId(0, "SeedingError"),
                        $"[FATAL ERROR] Error seeding database : {ex.Message}",
                        null,
                        (state, ex) => state.ToString());
                }
            }

            return false;
        }
    }
}