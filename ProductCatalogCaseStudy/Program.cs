using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductCatalogCaseStudy;
using ProductCatalogCaseStudy.Data;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using ProductCatalogCaseStudy.Repositories;
using ProductCatalogCaseStudy.Repositories.Contracts;
using ProductCatalogCaseStudy.Services;
using ProductCatalogCaseStudy.Services.Contracts;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ----------------------------------------------------------------------
// DB Connection (with connection parameters from env vars)
// ----------------------------------------------------------------------
var connectionString = new StringBuilder();
connectionString.Append($"Server={builder.Configuration["DB_HOST"]};");
connectionString.Append($"Database={builder.Configuration["DB_NAME"]};");
connectionString.Append($"User Id={builder.Configuration["DB_USER"]};");
connectionString.Append($"Password={builder.Configuration["DB_PASSWORD"]};");
connectionString.Append("TrustServerCertificate=True");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString.ToString()));

// ----------------------------------------------------------------------
// API Versioning Configuration
// ----------------------------------------------------------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    // Format the version as "'v'major" (e.g., v1, v2)
    options.GroupNameFormat = "'v'VV";
    // Substitutes the API version in the route template (e.g., /v1/products)
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddControllers();

// ----------------------------------------------------------------------
// OpenAPI documentation
// ----------------------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ----------------------------------------------------------------------
// Register Service and Data access layers implementations
// ----------------------------------------------------------------------
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// ----------------------------------------------------------------------
// Other services
// ----------------------------------------------------------------------
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Logging.AddConsole();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply pending migrations automatically when the container starts
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
    try
    {
        // An initial seed of data is inserted in the DB
        var productRepository = services.GetRequiredService<IProductRepository>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        if (ProductSeeder.Initialize(productRepository, "initial_seed_db.json", logger).Result)
        {
            logger.LogInformation("Database seeded with initial product data.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with initial data.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            // Dynamically define the Swagger endpoint for each version
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                    $"v{description.ApiVersion.ToString()}");
        }
    });
}

app.Run();
