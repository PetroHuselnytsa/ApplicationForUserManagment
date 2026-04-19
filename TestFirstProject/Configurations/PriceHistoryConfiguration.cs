using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("price_history");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(p => p.OldPrice)
            .HasColumnName("old_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.NewPrice)
            .HasColumnName("new_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired();

        builder.Property(p => p.ChangedByUserId)
            .HasColumnName("changed_by_user_id");

        builder.HasOne(p => p.Product)
            .WithMany(prod => prod.PriceHistories)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ProductId);
        builder.HasIndex(p => p.ChangedAt);
    }
}
