using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductApi.Database;
using ProductApi.Model;
using ProductApi.Helpers;
using ProductType = ProductApi.Model.Product;

namespace ProductApi.UnitTests
{
    public class ProductDbContextTests
    {
        private class FakeSystem : ISystem
        {
            public FakeSystem(Guid id, DateTimeOffset now)
            {
                GuidValue = id;
                Now = now;
            }

            private Guid GuidValue { get; }
            private DateTimeOffset Now { get; }

            public DateTimeOffset UtcNow => Now;

            public Guid NewGuid => GuidValue;
        }

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
                Assert.IsNotNull(dbProduct);
                Assert.AreEqual("Test Product", dbProduct!.Name);
                Assert.AreEqual(12.34M, dbProduct.Price);
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
                Assert.IsNotNull(p);
                p!.Name = "Updated";
                p.Price = 2.00M;
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.IsNotNull(p);
                Assert.AreEqual("Updated", p!.Name);
                Assert.AreEqual(2.00M, p.Price);
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
                Assert.IsNotNull(p);
                context.Products.Remove(p!);
                await context.SaveChangesAsync();
            }

            using (var context = new ProductDbContext(options))
            {
                var p = await context.Products.FindAsync(product.Id);
                Assert.IsNull(p);
            }
        }
    }
}
