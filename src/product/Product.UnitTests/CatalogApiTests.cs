using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductApi.Database;
using ProductApi.Model;
using ProductApi.Api;
using System.Text.Json;
using System.Linq;
using ProductType = ProductApi.Model.Product;

namespace ProductApi.UnitTests
{
    public class CatalogApiTests
    {
        private WebApplicationFactory<Program> CreateFactory(string dbName)
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the ProductDbContext registration with an in-memory DB for tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<ProductDbContext>(options => options.UseInMemoryDatabase(dbName));
                });
            });
        }

    private async System.Threading.Tasks.Task SeedAsync(WebApplicationFactory<Program> factory, params ProductType[] products)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            // Ensure DB is clean
            db.Products.RemoveRange(db.Products);
            db.Categories.RemoveRange(db.Categories);
            await db.SaveChangesAsync();

            // Ensure a category exists for any products that reference one
            var cat = new Category { Id = Guid.NewGuid(), Name = "Default" };
            db.Categories.Add(cat);

            foreach (var p in products)
            {
                if (p.CategoryId == Guid.Empty) p.CategoryId = cat.Id;
                db.Products.Add(p);
            }

            await db.SaveChangesAsync();
        }

        [Test]
        public async System.Threading.Tasks.Task Catalog_OnlyActiveProductsReturned()
        {
            var factory = CreateFactory("CatalogTestDb1");
            var p1 = new ProductType { Id = Guid.NewGuid(), Name = "A", Price = 1.0M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            var p2 = new ProductType { Id = Guid.NewGuid(), Name = "B", Price = 2.0M, Currency = "USD", IsActive = false, CreatedAt = DateTimeOffset.UtcNow };
            var p3 = new ProductType { Id = Guid.NewGuid(), Name = "C", Price = 3.0M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            await SeedAsync(factory, p1, p2, p3);

            var client = factory.CreateClient();
            var resp = await client.GetAsync("/api/catalog/products");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var page = await resp.Content.ReadFromJsonAsync<PagedResponse<ProductType>>();
            // PagedResponse<T> is a value type (record struct), so it cannot be null. Assert expected contents.
            Assert.That(page.TotalCount, Is.EqualTo(2));
            Assert.That(page.Items.All(i => i.IsActive), Is.True);
        }

        [Test]
        public async System.Threading.Tasks.Task Catalog_InvalidDynamicQuery_ReturnsBadRequest()
        {
            var factory = CreateFactory("CatalogTestDb2");
            await SeedAsync(factory);

            var client = factory.CreateClient();
            var resp = await client.GetAsync("/api/catalog/products?q=THIS IS INVALID");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.That(doc, Is.Not.Null);
            Assert.That(doc!.RootElement.TryGetProperty("error", out _), Is.True);
        }

        [Test]
        public async System.Threading.Tasks.Task Catalog_OrderBy_PriceDescending_Works()
        {
            var factory = CreateFactory("CatalogTestDb3");
            var p1 = new ProductType { Id = Guid.NewGuid(), Name = "A", Price = 5M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            var p2 = new ProductType { Id = Guid.NewGuid(), Name = "B", Price = 10M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            var p3 = new ProductType { Id = Guid.NewGuid(), Name = "C", Price = 1M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            await SeedAsync(factory, p1, p2, p3);

            var client = factory.CreateClient();
            var resp = await client.GetAsync("/api/catalog/products?orderBy=Price%20desc&pageSize=10");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var page = await resp.Content.ReadFromJsonAsync<PagedResponse<ProductType>>();
            var items = page.Items.ToList();
            Assert.That(items.Count, Is.EqualTo(3));
            Assert.That(items[0].Price, Is.EqualTo(10M));
            Assert.That(items[1].Price, Is.EqualTo(5M));
            Assert.That(items[2].Price, Is.EqualTo(1M));
        }

        [Test]
        public async System.Threading.Tasks.Task Catalog_Paging_Works()
        {
            var factory = CreateFactory("CatalogTestDb4");
            var products = Enumerable.Range(1, 3).Select(i => new ProductType { Id = Guid.NewGuid(), Name = $"P{i}", Price = i, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow }).ToArray();
            await SeedAsync(factory, products);

            var client = factory.CreateClient();
            var resp = await client.GetAsync("/api/catalog/products?page=2&pageSize=1");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var page = await resp.Content.ReadFromJsonAsync<PagedResponse<ProductType>>();
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(1));
            Assert.That(page.TotalCount, Is.EqualTo(3));
            Assert.That(page.TotalPages, Is.EqualTo(3));
            Assert.That(page.Items.Count(), Is.EqualTo(1));
        }
    }
}
