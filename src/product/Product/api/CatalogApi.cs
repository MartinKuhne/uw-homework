using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace ProductApi.Api
{
    public static class CatalogApi
    {
        public static RouteGroupBuilder MapCatalog(this RouteGroupBuilder group)
        {
            // Dynamic query endpoint: /products/query?q=<dynamic linq where>&orderBy=<order expression>&page=&pageSize=
            group.MapGet("/products", async (ProductDbContext db, IDistributedCache cache, string? q, string? orderBy, int page = 1, int pageSize = 20) =>
            {
                if (page < 1) page = 1;
                pageSize = System.Math.Clamp(pageSize, 1, 100);

                try
                {
                    // Compute a cache key from the query parameters
                    string rawKey = $"catalog:q={q ?? string.Empty}|o={orderBy ?? string.Empty}|p={page}|ps={pageSize}";
                    // Hash the key to keep it short and safe for cache stores
                    string cacheKey;
                    using (var sha = SHA256.Create())
                    {
                        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
                        cacheKey = "catalog:" + Convert.ToHexString(bytes);
                    }

                    // Try cache
                    var cached = await cache.GetAsync(cacheKey);
                    if (cached is not null)
                    {
                        var cachedJson = Encoding.UTF8.GetString(cached);
                        PagedResponse<ProductApi.Model.Product>? cachedResult = JsonSerializer.Deserialize<PagedResponse<ProductApi.Model.Product>>(cachedJson);
                        if (cachedResult.HasValue)
                        {
                            return Results.Ok(cachedResult.Value);
                        }
                    }

                    var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        // q is a Dynamic LINQ predicate, e.g. "Price > 10 and Currency == \"USD\""
                        query = query.Where(q);
                    }

                    // For the catalog, do not allow querying for inactive products
                    query = query.Where(item => item.IsActive == true);

                    if (!string.IsNullOrWhiteSpace(orderBy))
                    {
                        query = query.OrderBy(orderBy);
                    }

                    // Note this is a potentially expensive operation, and the need for it should be evaluated
                    var total = await query.CountAsync();
                    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                    var totalPages = (int)System.Math.Ceiling(total / (double)pageSize);
                    var result = new PagedResponse<ProductApi.Model.Product>(items, page, pageSize, total, totalPages);

                    // Serialize and store in cache for a short duration
                    try
                    {
                        var json = JsonSerializer.Serialize(result);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        var options = new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        };
                        await cache.SetAsync(cacheKey, bytes, options);
                    }
                    catch
                    {
                        // Swallow cache errors - caching is a best-effort
                    }

                    return Results.Ok(result);
                }
                catch (System.Linq.Dynamic.Core.Exceptions.ParseException ex)
                {
                    return Results.BadRequest(new { error = "Query failed", detail = ex.Message });
                }
            }).WithName("Catalog_QueryProducts").WithOpenApi().AllowAnonymous();

            return group;
        }
    }
}
