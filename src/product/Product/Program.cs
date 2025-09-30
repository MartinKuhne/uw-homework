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
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer definition so Swagger UI shows an authorize button
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Description = "Enter 'Bearer {token}' or just paste the JWT token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Bind feature flags from configuration
var featureFlags = new ProductApi.Configuration.FeatureFlags();
builder.Configuration.GetSection("FeatureFlags").Bind(featureFlags);
builder.Services.AddSingleton(featureFlags);

// Bind JWT options (optional)
var jwtOptions = new ProductApi.Configuration.JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOptions);

// If an authority/metadata or audience is configured, enable JWT Bearer authentication
if (!string.IsNullOrWhiteSpace(jwtOptions.Authority) || !string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        if (!string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
        {
            options.MetadataAddress = jwtOptions.MetadataAddress;
        }
        else if (!string.IsNullOrWhiteSpace(jwtOptions.Authority))
        {
            options.Authority = jwtOptions.Authority;
        }

        options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;

        // Ensure token validation parameters exist and explicitly disable audience validation
        options.TokenValidationParameters ??= new Microsoft.IdentityModel.Tokens.TokenValidationParameters();
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = false; // explicitly turn off audience validation
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;

        // We may still set an audience for other purposes, but validation will remain disabled
        if (!string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            options.Audience = jwtOptions.Audience;
        }
    });

    builder.Services.AddAuthorization();
}

// If admin scope is configured, add a policy that checks the scope/scp claim
if (!string.IsNullOrWhiteSpace(jwtOptions.AdminScope))
{
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminScopePolicy", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context =>
            {
                // Check both 'scope' (often space-separated) and 'scp' (array) claims
                var scopeClaim = context.User.FindFirst(c => c.Type == "scope" || c.Type == "scp");
                if (scopeClaim == null)
                {
                    return false;
                }

                var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return scopes.Contains(jwtOptions.AdminScope);
            });
        });
    });
}

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
    if (!string.IsNullOrWhiteSpace(jwtOptions.Authority) || !string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
    {
        if (!string.IsNullOrWhiteSpace(jwtOptions.AdminScope))
        {
            products.RequireAuthorization("AdminScopePolicy");
        }
        else
        {
            products.RequireAuthorization();
        }
    }
    products.MapProducts();

    // Category endpoints
    var categories = app.MapGroup("/api/categories");
    if (!string.IsNullOrWhiteSpace(jwtOptions.Authority) || !string.IsNullOrWhiteSpace(jwtOptions.MetadataAddress))
    {
        if (!string.IsNullOrWhiteSpace(jwtOptions.AdminScope))
        {
            categories.RequireAuthorization("AdminScopePolicy");
        }
        else
        {
            categories.RequireAuthorization();
        }
    }
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
