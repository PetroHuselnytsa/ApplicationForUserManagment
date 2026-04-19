using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.ToTable("stock_entries");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.ProductVariantId)
            .HasColumnName("product_variant_id")
            .IsRequired();

        builder.Property(s => s.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired();

        builder.Property(s => s.QuantityOnHand)
            .HasColumnName("quantity_on_hand")
            .IsRequired();

        builder.Property(s => s.QuantityReserved)
            .HasColumnName("quantity_reserved")
            .IsRequired();

        builder.Property(s => s.LowStockThreshold)
            .HasColumnName("low_stock_threshold")
            .HasDefaultValue(10);

        builder.Property(s => s.LastUpdated)
            .HasColumnName("last_updated")
            .IsRequired();

        builder.HasOne(s => s.ProductVariant)
            .WithMany(v => v.StockEntries)
            .HasForeignKey(s => s.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.StockEntries)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // One stock entry per variant per warehouse
        builder.HasIndex(s => new { s.ProductVariantId, s.WarehouseId }).IsUnique();
        builder.HasIndex(s => s.WarehouseId);
    }
}
