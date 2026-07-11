using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductApi.Database;
using ProductApi.Model;
using ProductApi.Helpers;
using ProductType = ProductApi.Model.Product;
using ProductApi.UnitTests.TestHelpers;

namespace ProductApi.UnitTests
{
    public class ProductDbContextTests
    {
        // Use the shared test helper FakeSystem in ProductApi.UnitTests.TestHelpers

        private DbContextOptions<ProductDbContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh
            // InMemory database instance per test
            return new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
        }

        [Test]
        public async System.Threading.Tasks.Task CanCreateAndReadProduct()
        {
            var options = CreateNewContextOptions();

            var sys = new FakeSystem(Guid.Parse("11111111-1111-1111-1111-111111111111"), DateTimeOffset.Parse("2025-09-30T00:00:00Z"));
            var product = new ProductType { Name = "Test Product", Price = 12.34M, Currency = "USD" };
            product.Id = sys.NewGuid;
            product.CreatedAt = sys.UtcNow;

            // Create
            using (var context = new ProductDbContext(options))
            {
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            // Read
            using (var context = new ProductDbContext(options))
            {
                var dbProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
                Assert.That(dbProduct, Is.Not.Null);
                Assert.That(dbProduct!.Name, Is.EqualTo("Test Product"));
                Assert.That(dbProduct.Price, Is.EqualTo(12.34M));
            }
        }

        [Test]
        public async System.Threading.Tasks.Task CanUpdateProduct()
        {
            var options = CreateNewContextOptions();

            var sys2 = new FakeSystem(Guid.Parse("22222222-2222-2222-2222-222222222222"), DateTimeOffset.Parse("2025-09-30T00:00:00Z"));
            var product = new ProductType { Name = "Old", Price = 1.00M, Currency = "USD" };
            product.Id = sys2.NewGuid;
            product.CreatedAt = sys2.UtcNow;

            using (var context = new ProductDbContext(options))
            {
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.That(p, Is.Not.Null);
                p!.Name = "Updated";
                p.Price = 2.00M;
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.That(p, Is.Not.Null);
                Assert.That(p!.Name, Is.EqualTo("Updated"));
                Assert.That(p.Price, Is.EqualTo(2.00M));
            }
        }

        [Test]
        public async System.Threading.Tasks.Task CanDeleteProduct()
        {
            var options = CreateNewContextOptions();

            var sys3 = new FakeSystem(Guid.Parse("33333333-3333-3333-3333-333333333333"), DateTimeOffset.Parse("2025-09-30T00:00:00Z"));
            var product = new ProductType { Name = "ToDelete", Price = 5.00M, Currency = "USD" };
            product.Id = sys3.NewGuid;
            product.CreatedAt = sys3.UtcNow;

            using (var context = new ProductDbContext(options))
            {
                context.Products.Add(product);
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.That(p, Is.Not.Null);
                context.Products.Remove(p!);
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.That(p, Is.Null);
            }
        }
    }
}
