using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using System.Linq.Dynamic.Core;

namespace ProductApi.Api
{
    public static class CatalogApi
    {
        public static RouteGroupBuilder MapCatalog(this RouteGroupBuilder group)
        {
            // Dynamic query endpoint: /products/query?q=<dynamic linq where>&orderBy=<order expression>&page=&pageSize=
            group.MapGet("/products", async (ProductDbContext db, string? q, string? orderBy, int page = 1, int pageSize = 20) =>
            {
                if (page < 1) page = 1;
                pageSize = System.Math.Clamp(pageSize, 1, 100);

                try
                {
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
