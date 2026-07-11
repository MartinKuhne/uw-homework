using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using ProductApi.Helpers;

namespace ProductApi.Api
{
    public static class ProductsApi
    {
        public static RouteGroupBuilder MapProducts(this RouteGroupBuilder group)
        {
            // List (include category) with pagination: ?page=1&pageSize=20
            group.MapGet("/", async (ProductDbContext db, int page = 1, int pageSize = 20) =>
            {
                // normalize paging parameters
                if (page < 1) page = 1;
                pageSize = System.Math.Clamp(pageSize, 1, 100);

                var query = db.Products.AsNoTracking().Include(p => p.Category);

                var total = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)System.Math.Ceiling(total / (double)pageSize);
                var result = new PagedResponse<ProductApi.Model.Product>(items, page, pageSize, total, totalPages);

                return Results.Ok(result);
            }).WithName("GetProducts").WithOpenApi().AllowAnonymous();

            // Get by id
            group.MapGet("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var p = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
                return p is not null ? Results.Ok(p) : Results.NotFound();
            }).WithName("GetProductById").WithOpenApi().AllowAnonymous();

            // Create
            group.MapPost("/", async (ProductApi.Model.Product product, ProductDbContext db, ISystem sys) =>
            {
                if (!await db.Categories.AnyAsync(c => c.Id == product.CategoryId))
                {
                    return Results.BadRequest($"Category with ID '{product.CategoryId}' does not exist.");
                }

                product.Id = product.Id == Guid.Empty ? sys.NewGuid : product.Id;
                product.CreatedAt = sys.UtcNow;
                // Only set foreign key; do not attach navigation property coming from client
                var toAdd = product;
                toAdd.Category = null;
                db.Products.Add(toAdd);
                await db.SaveChangesAsync();
                return Results.Created($"/api/products/{product.Id}", product);
            }).WithName("CreateProduct").WithOpenApi()
            .AllowAnonymous();

            // Update
            group.MapPut("/{id}", async (Guid id, ProductApi.Model.Product updated, ProductDbContext db, ISystem sys) =>
            {
                var existing = await db.Products.FindAsync(id);
                if (existing is null) return Results.NotFound();

                existing.Name = updated.Name;
                existing.Description = updated.Description;
                existing.Price = updated.Price;
                existing.Currency = updated.Currency;
                existing.CategoryId = updated.CategoryId;
                existing.IsActive = updated.IsActive;
                existing.WeightKg = updated.WeightKg;
                existing.WidthCm = updated.WidthCm;
                existing.HeightCm = updated.HeightCm;
                existing.DepthCm = updated.DepthCm;
                existing.UpdatedAt = sys.UtcNow;

                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("UpdateProduct").WithOpenApi()
            .AllowAnonymous();

            // Delete
            group.MapDelete("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var existing = await db.Products.FindAsync(id);
                if (existing is null) return Results.NotFound();
                db.Products.Remove(existing);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("DeleteProduct").WithOpenApi()
            .AllowAnonymous();

            return group;
        }
    }
}
