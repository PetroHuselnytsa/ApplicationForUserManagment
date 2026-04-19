using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.CouponId)
            .HasColumnName("coupon_id")
            .IsRequired();

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(u => u.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(u => u.UsedAt)
            .HasColumnName("used_at")
            .IsRequired();

        builder.HasOne(u => u.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => new { u.CouponId, u.UserId });
        builder.HasIndex(u => u.OrderId);
    }
}
