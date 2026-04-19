using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .HasColumnName("sku")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.BasePrice)
            .HasColumnName("base_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Weight)
            .HasColumnName("weight")
            .HasPrecision(10, 3);

        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(p => p.ImageUrls)
            .HasColumnName("image_urls")
            .HasMaxLength(2000);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.SKU).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BasePrice);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.IsActive, p.CategoryId, p.BasePrice });
    }
}
