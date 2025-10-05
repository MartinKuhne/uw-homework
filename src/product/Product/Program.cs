using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using ProductApi.Api;
using Serilog.AspNetCore;
using ProductApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add environment variables provider. Use a prefix so production env vars can be namespaced.
// Example: Jwt__WriteScope=uw-ecom-api/write will override configuration:Jwt:WriteScope
builder.Configuration.AddEnvironmentVariables();

// Configure logging (Serilog) from configuration using the centralized configurator
LoggingConfigurator.Configure(builder);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
ProductApi.Configuration.SwaggerConfigurator.Configure(builder.Services);

// Bind feature flags from configuration
var featureFlags = new ProductApi.Configuration.FeatureFlags();
builder.Configuration.GetSection("FeatureFlags").Bind(featureFlags);
builder.Services.AddSingleton(featureFlags);

// Bind JWT options (optional)
var jwtOptions = new ProductApi.Configuration.JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);

// Configure JWT authentication and authorization policies
ProductApi.Configuration.JwtConfigurator.ConfigureJwt(builder, jwtOptions);

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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at app root in development
    });
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware (ensure this runs before endpoint routing)
app.UseAuthentication();
app.UseAuthorization();

// Ensure database is created at startup (for dev). In production, use migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

// Admin endpoints (enabled via feature flag)
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

// Simple health check that returns plain text
app.MapGet("/health", () => Results.Text("healthy")).WithName("Health").AllowAnonymous();

try
{
    var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    // Log environment variables on startup as structured data (be careful not to log secrets in production)
    try
    {
        var envVars = Environment.GetEnvironmentVariables();
        var envDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (DictionaryEntry de in envVars)
        {
            var key = de.Key?.ToString() ?? string.Empty;
            var value = de.Value?.ToString() ?? string.Empty;
            envDict[key] = value;
        }

        // Structured log - `@Env` will preserve the dictionary structure for Serilog sinks
        logger.LogInformation("Environment variables: {@Env}", envDict);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to enumerate environment variables");
    }

    logger.LogInformation("Starting Product API");
    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
    logger.LogCritical(ex, "Host terminated unexpectedly");
    throw;
}
