using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .HasColumnName("shipping_address")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(o => o.BillingAddress)
            .HasColumnName("billing_address")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(o => o.SubTotal)
            .HasColumnName("sub_total")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingCost)
            .HasColumnName("shipping_cost")
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.ShippingMethod)
            .HasColumnName("shipping_method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CouponId)
            .HasColumnName("coupon_id");

        builder.Property(o => o.LoyaltyPointsUsed)
            .HasColumnName("loyalty_points_used");

        builder.Property(o => o.LoyaltyDiscount)
            .HasColumnName("loyalty_discount")
            .HasPrecision(18, 2);

        builder.Property(o => o.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Optimistic concurrency
        builder.Property(o => o.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Coupon)
            .WithMany()
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => new { o.UserId, o.CreatedAt });
    }
}
