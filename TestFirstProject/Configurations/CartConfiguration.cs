using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(c => c.AppliedCouponId)
            .HasColumnName("applied_coupon_id");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.LastActivityAt)
            .HasColumnName("last_activity_at")
            .IsRequired();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AppliedCoupon)
            .WithMany()
            .HasForeignKey(c => c.AppliedCouponId)
            .OnDelete(DeleteBehavior.SetNull);

        // One active cart per user
        builder.HasIndex(c => c.UserId).IsUnique();
        builder.HasIndex(c => c.LastActivityAt);
    }
}
