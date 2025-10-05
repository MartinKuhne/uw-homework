using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductApi.Database;
using ProductApi.Model;
using ProductApi.Api;
using ProductType = ProductApi.Model.Product;
using System.Linq;

namespace ProductApi.UnitTests
{
    public class ProductsApiTests
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
            db.Products.RemoveRange(db.Products);
            db.Categories.RemoveRange(db.Categories);
            await db.SaveChangesAsync();

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
        public async System.Threading.Tasks.Task GetProducts_Paging_ReturnsCorrectPage()
        {
            var factory = CreateFactory("ProductsTestDb1");
            var products = Enumerable.Range(1, 5).Select(i => new ProductType { Id = Guid.NewGuid(), Name = $"P{i}", Price = i, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow }).ToArray();
            await SeedAsync(factory, products);

            var client = factory.CreateClient();
            var resp = await client.GetAsync("/api/products?page=2&pageSize=2");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var page = await resp.Content.ReadFromJsonAsync<PagedResponse<ProductType>>();
            Assert.That(page.Page, Is.EqualTo(2));
            Assert.That(page.PageSize, Is.EqualTo(2));
            Assert.That(page.TotalCount, Is.EqualTo(5));
            Assert.That(page.TotalPages, Is.EqualTo(3));
            Assert.That(page.Items.Count(), Is.EqualTo(2));
        }

        [Test]
        public async System.Threading.Tasks.Task GetProductById_ReturnsProduct()
        {
            var factory = CreateFactory("ProductsTestDb2");
            var p = new ProductType { Id = Guid.NewGuid(), Name = "X", Price = 9.99M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            await SeedAsync(factory, p);

            var client = factory.CreateClient();
            var resp = await client.GetAsync($"/api/products/{p.Id}");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var got = await resp.Content.ReadFromJsonAsync<ProductType>();
            Assert.That(got, Is.Not.Null);
            Assert.That(got!.Id, Is.EqualTo(p.Id));
            Assert.That(got.Name, Is.EqualTo(p.Name));
        }

        [Test]
    public async System.Threading.Tasks.Task CreateProduct_WithMissingCategory_ReturnsBadRequest()
        {
            var factory = CreateFactory("ProductsTestDb3");
            await SeedAsync(factory); // creates a default category but we'll use a different id

            var client = factory.CreateClient();

            var newProduct = new ProductType { Name = "NewP", Price = 1.23M, Currency = "USD", CategoryId = Guid.NewGuid(), IsActive = true };
            var resp = await client.PostAsJsonAsync("/api/products/", newProduct);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
    public async System.Threading.Tasks.Task CreateProduct_SucceedsAndPersists()
        {
            var factory = CreateFactory("ProductsTestDb4");
            await SeedAsync(factory); // creates default category

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var cat = await db.Categories.FirstAsync();

            var client = factory.CreateClient();
            var newProduct = new ProductType { Name = "CreateP", Price = 2.5M, Currency = "USD", CategoryId = cat.Id, IsActive = true };
            var resp = await client.PostAsJsonAsync("/api/products/", newProduct);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var created = await resp.Content.ReadFromJsonAsync<ProductType>();
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.Id, Is.Not.EqualTo(Guid.Empty));

            // Verify persisted via GET
            var get = await client.GetAsync($"/api/products/{created.Id}");
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async System.Threading.Tasks.Task UpdateProduct_NotFound_ReturnsNotFound()
        {
            var factory = CreateFactory("ProductsTestDb5");
            await SeedAsync(factory);

            var client = factory.CreateClient();
            var updated = new ProductType { Id = Guid.NewGuid(), Name = "U", Price = 1M, Currency = "USD", IsActive = true };
            var resp = await client.PutAsJsonAsync($"/api/products/{updated.Id}", updated);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async System.Threading.Tasks.Task DeleteProduct_Existing_ReturnsNoContent()
        {
            var factory = CreateFactory("ProductsTestDb6");
            var p = new ProductType { Id = Guid.NewGuid(), Name = "D", Price = 3M, Currency = "USD", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
            await SeedAsync(factory, p);

            var client = factory.CreateClient();
            var resp = await client.DeleteAsync($"/api/products/{p.Id}");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var get = await client.GetAsync($"/api/products/{p.Id}");
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
