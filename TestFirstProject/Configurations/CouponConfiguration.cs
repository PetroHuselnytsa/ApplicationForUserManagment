using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;

namespace TestFirstProject.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.DiscountValue)
            .HasColumnName("discount_value")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.MinOrderValue)
            .HasColumnName("min_order_value")
            .HasPrecision(18, 2);

        builder.Property(c => c.MaxUses)
            .HasColumnName("max_uses");

        builder.Property(c => c.CurrentUses)
            .HasColumnName("current_uses");

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
