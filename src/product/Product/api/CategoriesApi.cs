using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProductApi.Database;
using ProductApi.Helpers;

namespace ProductApi.Api
{
    public static class CategoriesApi
    {
        public static RouteGroupBuilder MapCategories(this RouteGroupBuilder group)
        {
            // List categories
            group.MapGet("/", async (ProductDbContext db) =>
                await db.Categories.AsNoTracking().ToListAsync())
                .WithName("GetCategories").WithOpenApi();

            // Get by id
            group.MapGet("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var c = await db.Categories.FindAsync(id);
                return c is not null ? Results.Ok(c) : Results.NotFound();
            }).WithName("GetCategoryById").WithOpenApi();

            // Create - return BadRequest if a category with the same name already exists
            group.MapPost("/", async (ProductApi.Model.Category category, ProductDbContext db, ISystem sys) =>
            {
                var name = (category.Name ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(name))
                {
                    return Results.BadRequest("Category name is required.");
                }

                // Load existing names and compare in-memory using StringComparer.OrdinalIgnoreCase
                var existingNames = await db.Categories.AsNoTracking()
                    .Where(c => c.Name != null)
                    .Select(c => c.Name!)
                    .ToListAsync();

                var exists = existingNames.Any(n => StringComparer.OrdinalIgnoreCase.Equals(n, name));

                if (exists)
                {
                    return Results.BadRequest($"A category with the name '{name}' already exists.");
                }

                category.Id = category.Id == Guid.Empty ? sys.NewGuid : category.Id;
                category.Name = name; // normalize
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                return Results.Created($"/api/categories/{category.Id}", category);
            }).WithName("CreateCategory").WithOpenApi();

            // Update
            group.MapPut("/{id}", async (Guid id, ProductApi.Model.Category updated, ProductDbContext db) =>
            {
                var existing = await db.Categories.FindAsync(id);
                if (existing is null) return Results.NotFound();
                existing.Name = updated.Name;
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("UpdateCategory").WithOpenApi();

            // Delete
            group.MapDelete("/{id}", async (Guid id, ProductDbContext db) =>
            {
                var existing = await db.Categories.FindAsync(id);
                if (existing is null) return Results.NotFound();
                db.Categories.Remove(existing);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).WithName("DeleteCategory").WithOpenApi();

            return group;
        }
    }
}
