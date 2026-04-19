using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("inventory_transactions");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(t => t.StockEntryId).HasColumnName("stock_entry_id");
            builder.Property(t => t.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(t => t.QuantityChange).HasColumnName("quantity_change");
            builder.Property(t => t.Reason).HasColumnName("reason").HasMaxLength(500);
            builder.Property(t => t.ReferenceId).HasColumnName("reference_id").HasMaxLength(100);
            builder.Property(t => t.CreatedAt).HasColumnName("created_at");

            builder.HasOne(t => t.StockEntry)
                   .WithMany()
                   .HasForeignKey(t => t.StockEntryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.StockEntryId);
            builder.HasIndex(t => t.CreatedAt);
        }
    }
}
