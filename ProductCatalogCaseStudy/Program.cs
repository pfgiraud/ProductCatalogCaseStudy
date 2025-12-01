using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProductCatalogCaseStudy.Data;
using ProductCatalogCaseStudy.DTO;
using ProductCatalogCaseStudy.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Load connection string parameters from env vars for db connection
var connectionString = new StringBuilder();
connectionString.Append($"Server={builder.Configuration["DB_HOST"]};");
connectionString.Append($"Database={builder.Configuration["DB_NAME"]};");
connectionString.Append($"User Id={builder.Configuration["DB_USER"]};");
connectionString.Append($"Password={builder.Configuration["DB_PASSWORD"]};");
connectionString.Append("TrustServerCertificate=True");

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString.ToString()));
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Product Catalog API",
        Description = "An ASP.NET Core Web API for a catalog of products. Study Case part of the Alza recruitement process",
        Contact = new OpenApiContact
        {
            Name = "Author",
            Url = new Uri("https://www.linkedin.com/in/pierre-francois-giraud-835288108/")
        }
    });
});

builder.Logging.AddConsole();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Apply pending migrations automatically when the container starts
        context.Database.Migrate();

        // An initial seed of data is inserted in the DB
        var logger = services.GetRequiredService<ILogger<Program>>();
        if(ProductSeeder.Initialize(context, "initial_seed_db.json", logger))
        {
            logger.LogInformation("Database seeded with initial product data.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.Run();
