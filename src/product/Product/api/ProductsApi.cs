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
            // List
            group.MapGet("/", async (ProductDbContext db) =>
                await db.Products.AsNoTracking().ToListAsync())
                .WithName("GetProducts").WithOpenApi();

            // Get by id
            group.MapGet("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var p = await db.Products.FindAsync(id);
                return p is not null ? Results.Ok(p) : Results.NotFound();
            }).WithName("GetProductById").WithOpenApi();

            // Create
                group.MapPost("/", async (ProductApi.Model.Product product, ProductDbContext db, ISystem sys) =>
                {
                    product.Id = product.Id == Guid.Empty ? sys.NewGuid : product.Id;
                    product.CreatedAt = sys.UtcNow;
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                    return Results.Created($"/api/products/{product.Id}", product);
                }).WithName("CreateProduct").WithOpenApi();

            // Update
                group.MapPut("/{id}", async (Guid id, ProductApi.Model.Product updated, ProductDbContext db, ISystem sys) =>
                {
                var existing = await db.Products.FindAsync(id);
                if (existing is null) return Results.NotFound();

                existing.Name = updated.Name;
                existing.Description = updated.Description;
                existing.Price = updated.Price;
                existing.Currency = updated.Currency;
                existing.Category = updated.Category;
                existing.IsActive = updated.IsActive;
                existing.WeightKg = updated.WeightKg;
                existing.WidthCm = updated.WidthCm;
                existing.HeightCm = updated.HeightCm;
                existing.DepthCm = updated.DepthCm;
                existing.UpdatedAt = sys.UtcNow;

                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("UpdateProduct").WithOpenApi();

            // Delete
            group.MapDelete("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var existing = await db.Products.FindAsync(id);
                if (existing is null) return Results.NotFound();
                db.Products.Remove(existing);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("DeleteProduct").WithOpenApi();

            return group;
        }
    }
}
