using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ProductApi.Database;
using ProductApi.Model;
using ProductType = ProductApi.Model.Product;

namespace ProductApi.UnitTests
{
    public class ProductDbContextTests
    {
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

            var product = new ProductType { Name = "Test Product", Price = 12.34M, Currency = "USD" };

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

            var product = new ProductType { Name = "Old", Price = 1.00M, Currency = "USD" };

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

            var product = new ProductType { Name = "ToDelete", Price = 5.00M, Currency = "USD" };

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
