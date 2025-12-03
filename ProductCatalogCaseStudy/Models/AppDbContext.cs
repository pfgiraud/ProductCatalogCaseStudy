using Microsoft.EntityFrameworkCore;

namespace ProductCatalogCaseStudy.Models
{
#pragma warning disable CS1591 // XML Comment missing for public member
    /// <summary>
    /// The database context for Entity Framework Core.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }
#pragma warning restore CS1591 // XML Comment missing for public member
}