using Microsoft.EntityFrameworkCore;
using ProductCatalogCaseStudy.Models;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = new StringBuilder();
connectionString.Append($"Server={builder.Configuration["DB_HOST"]};");
connectionString.Append($"Database={builder.Configuration["DB_NAME"]};");
connectionString.Append($"User Id={builder.Configuration["DB_USER"]};");
connectionString.Append($"Password={builder.Configuration["DB_PASSWORD"]};");
connectionString.Append("TrustServerCertificate=True");

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString.ToString()));
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddOpenApi();

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

app.Run();
