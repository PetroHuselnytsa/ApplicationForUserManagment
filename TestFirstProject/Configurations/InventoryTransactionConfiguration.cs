using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("inventory_transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.StockEntryId)
            .HasColumnName("stock_entry_id")
            .IsRequired();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.QuantityChange)
            .HasColumnName("quantity_change")
            .IsRequired();

        builder.Property(t => t.Reason)
            .HasColumnName("reason")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.OrderId)
            .HasColumnName("order_id");

        builder.Property(t => t.PerformedByUserId)
            .HasColumnName("performed_by_user_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(t => t.StockEntry)
            .WithMany()
            .HasForeignKey(t => t.StockEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.StockEntryId);
        builder.HasIndex(t => t.OrderId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
