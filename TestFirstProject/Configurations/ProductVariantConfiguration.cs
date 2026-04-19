using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("product_variants");

            builder.HasKey(v => v.Id);
            builder.Property(v => v.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(v => v.ProductId).HasColumnName("product_id");
            builder.Property(v => v.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
            builder.Property(v => v.Size).HasColumnName("size").HasMaxLength(50);
            builder.Property(v => v.Color).HasColumnName("color").HasMaxLength(50);
            builder.Property(v => v.Material).HasColumnName("material").HasMaxLength(50);
            builder.Property(v => v.PriceDelta).HasColumnName("price_delta").HasColumnType("decimal(18,2)");
            builder.Property(v => v.IsActive).HasColumnName("is_active");

            builder.HasOne(v => v.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => v.Sku).IsUnique();
            builder.HasIndex(v => v.ProductId);

            // Seed variants
            builder.HasData(
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000001"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000001"), Sku = "PHONE-X-BLK", Color = "Black", PriceDelta = 0m },
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000002"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000001"), Sku = "PHONE-X-WHT", Color = "White", PriceDelta = 50m },
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000003"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000002"), Sku = "LAPTOP-PRO-16", Size = "16 inch", PriceDelta = 200m },
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000004"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000002"), Sku = "LAPTOP-PRO-14", Size = "14 inch", PriceDelta = 0m },
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000005"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000003"), Sku = "TSHIRT-CLS-M-BLK", Size = "M", Color = "Black", Material = "Cotton", PriceDelta = 0m },
                new ProductVariant { Id = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000006"), ProductId = Guid.Parse("b1b2c3d4-0002-0002-0002-000000000003"), Sku = "TSHIRT-CLS-L-WHT", Size = "L", Color = "White", Material = "Cotton", PriceDelta = 5m }
            );
        }
    }
}
