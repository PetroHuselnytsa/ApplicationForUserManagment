using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(o => o.OrderNumber).HasColumnName("order_number").IsRequired().HasMaxLength(20);
            builder.Property(o => o.UserId).HasColumnName("user_id");
            builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.ShippingAddress).HasColumnName("shipping_address").IsRequired().HasMaxLength(500);
            builder.Property(o => o.BillingAddress).HasColumnName("billing_address").IsRequired().HasMaxLength(500);
            builder.Property(o => o.SubTotal).HasColumnName("sub_total").HasColumnType("decimal(18,2)");
            builder.Property(o => o.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
            builder.Property(o => o.ShippingCost).HasColumnName("shipping_cost").HasColumnType("decimal(18,2)");
            builder.Property(o => o.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
            builder.Property(o => o.CouponCode).HasColumnName("coupon_code").HasMaxLength(50);
            builder.Property(o => o.ShippingMethod).HasColumnName("shipping_method").HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.CreatedAt).HasColumnName("created_at");
            builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

            builder.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.CreatedAt);
        }
    }
}
