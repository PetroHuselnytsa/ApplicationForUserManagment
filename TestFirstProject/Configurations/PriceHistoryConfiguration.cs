using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
    {
        public void Configure(EntityTypeBuilder<PriceHistory> builder)
        {
            builder.ToTable("price_history");

            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(h => h.ProductId).HasColumnName("product_id");
            builder.Property(h => h.OldPrice).HasColumnName("old_price").HasColumnType("decimal(18,2)");
            builder.Property(h => h.NewPrice).HasColumnName("new_price").HasColumnType("decimal(18,2)");
            builder.Property(h => h.ChangedAt).HasColumnName("changed_at");
            builder.Property(h => h.ChangedBy).HasColumnName("changed_by").HasMaxLength(100);

            builder.HasOne(h => h.Product).WithMany(p => p.PriceHistories).HasForeignKey(h => h.ProductId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(h => h.ProductId);
            builder.HasIndex(h => h.ChangedAt);
        }
    }
}
