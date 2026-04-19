using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class EcomProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(2000);
            builder.Property(p => p.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
            builder.Property(p => p.BasePrice).HasColumnName("base_price").HasColumnType("decimal(18,2)");
            builder.Property(p => p.Weight).HasColumnName("weight").HasColumnType("decimal(10,3)");
            builder.Property(p => p.IsActive).HasColumnName("is_active");
            builder.Property(p => p.CreatedAt).HasColumnName("created_at");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            builder.Property(p => p.CategoryId).HasColumnName("category_id");
            builder.Property(p => p.ImageUrls).HasColumnName("image_urls").HasColumnType("jsonb");

            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BasePrice);
            builder.HasIndex(p => p.Name);

            // Seed sample products
            builder.HasData(
                new Product
                {
                    Id = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000001"),
                    Name = "Smartphone X",
                    Description = "Latest smartphone with advanced features",
                    Sku = "PHONE-X-001",
                    BasePrice = 999.99m,
                    Weight = 0.2m,
                    CategoryId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000003"),
                    ImageUrls = new List<string> { "https://example.com/phone-x.jpg" }
                },
                new Product
                {
                    Id = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000002"),
                    Name = "Laptop Pro",
                    Description = "Professional-grade laptop",
                    Sku = "LAPTOP-PRO-001",
                    BasePrice = 1499.99m,
                    Weight = 1.8m,
                    CategoryId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000004"),
                    ImageUrls = new List<string> { "https://example.com/laptop-pro.jpg" }
                },
                new Product
                {
                    Id = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000003"),
                    Name = "Classic T-Shirt",
                    Description = "Comfortable cotton t-shirt",
                    Sku = "TSHIRT-CLS-001",
                    BasePrice = 29.99m,
                    Weight = 0.3m,
                    CategoryId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000005"),
                    ImageUrls = new List<string> { "https://example.com/tshirt.jpg" }
                }
            );
        }
    }
}
