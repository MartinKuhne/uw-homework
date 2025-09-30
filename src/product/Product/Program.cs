using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using ProductApi.Api;
using Serilog.AspNetCore;
using ProductApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure logging (Serilog) from configuration using the centralized configurator
LoggingConfigurator.Configure(builder);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind feature flags from configuration
var featureFlags = new ProductApi.Configuration.FeatureFlags();
builder.Configuration.GetSection("FeatureFlags").Bind(featureFlags);
builder.Services.AddSingleton(featureFlags);

// Register system helper for getting time and guids
builder.Services.AddSingleton<ProductApi.Helpers.ISystem, ProductApi.Helpers.SystemImpl>();

// Register EF Core ProductDbContext using SQLite. Connection string comes from configuration
// (ConnectionStrings:ProductDb). If not present, the default in appsettings.json will be used.
var connectionString = builder.Configuration.GetConnectionString("ProductDb");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<ProductApi.Database.ProductDbContext>(options =>
        options.UseSqlServer(connectionString));
}

var app = builder.Build();

app.UseMiddleware<RequestHeaderLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ensure database is created at startup (for dev). In production, use migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

// Read-only catalog endpoints (enabled via feature flag)
if (featureFlags.EnableAdminApi)
{
    // Product CRUD endpoints mapped from separate file
    var products = app.MapGroup("/api/products");
    products.MapProducts();

    // Category endpoints
    var categories = app.MapGroup("/api/categories");
    categories.MapCategories();
}

// Read-only catalog endpoints (enabled via feature flag)
if (featureFlags.EnableCatalogApi)
{
    var catalog = app.MapGroup("/api/catalog");
    catalog.MapCatalog();
}

try
{
    var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger.LogInformation("Starting Product API");
    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger.LogCritical(ex, "Host terminated unexpectedly");
    throw;
}
