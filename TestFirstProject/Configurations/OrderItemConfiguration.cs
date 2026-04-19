using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(oi => oi.OrderId).HasColumnName("order_id");
            builder.Property(oi => oi.ProductVariantId).HasColumnName("product_variant_id");
            builder.Property(oi => oi.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(200);
            builder.Property(oi => oi.VariantSku).HasColumnName("variant_sku").IsRequired().HasMaxLength(50);
            builder.Property(oi => oi.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.Quantity).HasColumnName("quantity");
            builder.Property(oi => oi.LineTotal).HasColumnName("line_total").HasColumnType("decimal(18,2)");

            builder.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(oi => oi.ProductVariant).WithMany().HasForeignKey(oi => oi.ProductVariantId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(oi => oi.OrderId);
        }
    }
}
