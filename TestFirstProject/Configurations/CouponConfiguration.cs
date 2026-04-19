using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("coupons");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(c => c.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            builder.Property(c => c.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Value).HasColumnName("value").HasColumnType("decimal(18,2)");
            builder.Property(c => c.MinOrderValue).HasColumnName("min_order_value").HasColumnType("decimal(18,2)");
            builder.Property(c => c.MaxUses).HasColumnName("max_uses");
            builder.Property(c => c.TimesUsed).HasColumnName("times_used");
            builder.Property(c => c.ExpiresAt).HasColumnName("expires_at");
            builder.Property(c => c.IsActive).HasColumnName("is_active");
            builder.Property(c => c.CreatedAt).HasColumnName("created_at");

            builder.HasIndex(c => c.Code).IsUnique();
        }
    }
}
