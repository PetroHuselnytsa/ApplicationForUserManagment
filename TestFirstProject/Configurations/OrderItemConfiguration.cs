using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(i => i.ProductVariantId)
            .HasColumnName("product_variant_id")
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasColumnName("product_name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.VariantDescription)
            .HasColumnName("variant_description")
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.TotalPrice)
            .HasColumnName("total_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OrderId);
    }
}
