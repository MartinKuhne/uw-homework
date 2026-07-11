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
        public DbSet<ProductApi.Model.Category> Categories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var product = modelBuilder.Entity<ProductEntity>();

            product.HasKey(p => p.Id);
            product.Property(p => p.Name).IsRequired().HasMaxLength(200);
            product.Property(p => p.Description).HasMaxLength(2000);
            product.Property(p => p.Price).HasColumnType("decimal(18,2)");
            product.Property(p => p.Currency).HasMaxLength(3).IsRequired();
            product.Property(p => p.IsActive).HasDefaultValue(true);
            product.Property(p => p.CreatedAt).IsRequired();
            product.Property(p => p.UpdatedAt);

            // Optional physical properties
            product.Property(p => p.WeightKg);
            product.Property(p => p.WidthCm);
            product.Property(p => p.HeightCm);
            product.Property(p => p.DepthCm);

            // Category relationship: optional many-to-one
            product.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey("CategoryId")
                .IsRequired(false);

            // Indexes for common query patterns
            product.HasIndex(p => p.Name).HasDatabaseName("IX_Products_Name");
            product.HasIndex(p => p.Price).HasDatabaseName("IX_Products_Price");
            product.HasIndex(p => p.IsActive).HasDatabaseName("IX_Products_IsActive");

            // Category entity configuration
            var category = modelBuilder.Entity<Category>();
            category.HasKey(c => c.Id);
            category.Property(c => c.Name).IsRequired().HasMaxLength(200);
            // Index on name for faster lookup and enforce uniqueness
            category.HasIndex(c => c.Name).HasDatabaseName("IX_Categories_Name").IsUnique();

            // Note: If you add collection properties (e.g. Tags) later, consider storing them
            // as JSON with a conversion for SQLite or a separate join table for relational DBs.
        }
    }
}
