using Microsoft.EntityFrameworkCore;
using ProductApi.Model;
using ProductEntity = ProductApi.Model.Product;

namespace ProductApi.Database
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

    public DbSet<ProductEntity> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var product = modelBuilder.Entity<ProductEntity>();

            product.HasKey(p => p.Id);
            product.Property(p => p.Name).IsRequired().HasMaxLength(200);
            product.Property(p => p.Description).HasMaxLength(2000);
            product.Property(p => p.Price).HasColumnType("decimal(18,2)");
            product.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            product.Property(p => p.Category).HasMaxLength(200);
            product.Property(p => p.IsActive).HasDefaultValue(true);
            product.Property(p => p.CreatedAt).IsRequired();
            product.Property(p => p.UpdatedAt);

            // Optional physical properties
            product.Property(p => p.WeightKg);
            product.Property(p => p.WidthCm);
            product.Property(p => p.HeightCm);
            product.Property(p => p.DepthCm);

          // Note: If you add collection properties (e.g. Tags) later, consider storing them
          // as JSON with a conversion for SQLite or a separate join table for relational DBs.
        }
    }
}
