using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("carts");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(c => c.UserId).HasColumnName("user_id");
            builder.Property(c => c.CreatedAt).HasColumnName("created_at");
            builder.Property(c => c.LastActivityAt).HasColumnName("last_activity_at");
            builder.Property(c => c.CouponCode).HasColumnName("coupon_code").HasMaxLength(50);

            builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.UserId).IsUnique();
            builder.HasIndex(c => c.LastActivityAt);
        }
    }
}
