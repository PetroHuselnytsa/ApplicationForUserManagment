using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
    {
        public void Configure(EntityTypeBuilder<StockEntry> builder)
        {
            builder.ToTable("stock_entries");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(s => s.ProductVariantId).HasColumnName("product_variant_id");
            builder.Property(s => s.WarehouseId).HasColumnName("warehouse_id");
            builder.Property(s => s.QuantityOnHand).HasColumnName("quantity_on_hand");
            builder.Property(s => s.QuantityReserved).HasColumnName("quantity_reserved");
            builder.Property(s => s.LowStockThreshold).HasColumnName("low_stock_threshold");

            builder.HasOne(s => s.ProductVariant)
                   .WithMany(v => v.StockEntries)
                   .HasForeignKey(s => s.ProductVariantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Warehouse)
                   .WithMany(w => w.StockEntries)
                   .HasForeignKey(s => s.WarehouseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.ProductVariantId, s.WarehouseId }).IsUnique();

            // Seed stock for sample products in main warehouse
            var mainWarehouseId = Guid.Parse("d1d2d3d4-0004-0004-0004-000000000001");
            builder.HasData(
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000001"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000001"), WarehouseId = mainWarehouseId, QuantityOnHand = 100, LowStockThreshold = 10 },
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000002"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000002"), WarehouseId = mainWarehouseId, QuantityOnHand = 75, LowStockThreshold = 10 },
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000003"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000003"), WarehouseId = mainWarehouseId, QuantityOnHand = 50, LowStockThreshold = 5 },
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000004"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000004"), WarehouseId = mainWarehouseId, QuantityOnHand = 50, LowStockThreshold = 5 },
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000005"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000005"), WarehouseId = mainWarehouseId, QuantityOnHand = 200, LowStockThreshold = 20 },
                new StockEntry { Id = Guid.Parse("e1e2e3e4-0005-0005-0005-000000000006"), ProductVariantId = Guid.Parse("c1c2c3c4-0003-0003-0003-000000000006"), WarehouseId = mainWarehouseId, QuantityOnHand = 150, LowStockThreshold = 20 }
            );
        }
    }
}
