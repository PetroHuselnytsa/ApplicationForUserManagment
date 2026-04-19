using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(v => v.SKU)
            .HasColumnName("sku")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Size)
            .HasColumnName("size")
            .HasMaxLength(30);

        builder.Property(v => v.Color)
            .HasColumnName("color")
            .HasMaxLength(30);

        builder.Property(v => v.Material)
            .HasColumnName("material")
            .HasMaxLength(50);

        builder.Property(v => v.PriceDelta)
            .HasColumnName("price_delta")
            .HasPrecision(18, 2);

        builder.Property(v => v.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.SKU).IsUnique();
        builder.HasIndex(v => v.ProductId);
    }
}
