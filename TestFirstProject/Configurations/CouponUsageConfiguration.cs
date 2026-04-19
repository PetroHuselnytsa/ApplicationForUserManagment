using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
    {
        public void Configure(EntityTypeBuilder<CouponUsage> builder)
        {
            builder.ToTable("coupon_usages");

            builder.HasKey(cu => cu.Id);
            builder.Property(cu => cu.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(cu => cu.CouponId).HasColumnName("coupon_id");
            builder.Property(cu => cu.UserId).HasColumnName("user_id");
            builder.Property(cu => cu.OrderId).HasColumnName("order_id");
            builder.Property(cu => cu.UsedAt).HasColumnName("used_at");

            builder.HasOne(cu => cu.Coupon).WithMany(c => c.Usages).HasForeignKey(cu => cu.CouponId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(cu => cu.User).WithMany().HasForeignKey(cu => cu.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(cu => cu.Order).WithMany().HasForeignKey(cu => cu.OrderId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cu => new { cu.CouponId, cu.UserId });
        }
    }
}
