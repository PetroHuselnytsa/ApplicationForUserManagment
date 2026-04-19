using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
{
    public void Configure(EntityTypeBuilder<FlashSale> builder)
    {
        builder.ToTable("flash_sales");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(f => f.SalePrice)
            .HasColumnName("sale_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(f => f.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(f => f.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(false);

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(f => f.Product)
            .WithMany(p => p.FlashSales)
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.ProductId);
        builder.HasIndex(f => new { f.IsActive, f.StartTime, f.EndTime });
    }
}
