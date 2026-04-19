using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("product_reviews");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(r => r.ProductId).HasColumnName("product_id");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.Rating).HasColumnName("rating");
            builder.Property(r => r.Text).HasColumnName("text").HasMaxLength(2000);
            builder.Property(r => r.IsVerifiedPurchase).HasColumnName("is_verified_purchase");
            builder.Property(r => r.CreatedAt).HasColumnName("created_at");

            builder.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
        }
    }
}
