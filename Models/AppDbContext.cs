using Microsoft.EntityFrameworkCore;

namespace ProductCatalogCaseStudy.Models
{
    /// <summary>
    /// The database context for Entity Framework Core.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
    }
}