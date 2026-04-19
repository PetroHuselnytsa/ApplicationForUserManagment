using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class RestockRequestConfiguration : IEntityTypeConfiguration<RestockRequest>
{
    public void Configure(EntityTypeBuilder<RestockRequest> builder)
    {
        builder.ToTable("restock_requests");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(r => r.ProductVariantId)
            .HasColumnName("product_variant_id")
            .IsRequired();

        builder.Property(r => r.WarehouseId)
            .HasColumnName("warehouse_id")
            .IsRequired();

        builder.Property(r => r.RequestedQuantity)
            .HasColumnName("requested_quantity")
            .IsRequired();

        builder.Property(r => r.FulfilledQuantity)
            .HasColumnName("fulfilled_quantity");

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.RequestedByUserId)
            .HasColumnName("requested_by_user_id")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.FulfilledAt)
            .HasColumnName("fulfilled_at");

        builder.HasOne(r => r.ProductVariant)
            .WithMany()
            .HasForeignKey(r => r.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
    }
}
