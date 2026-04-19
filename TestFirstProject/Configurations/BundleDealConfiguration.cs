using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class BundleDealConfiguration : IEntityTypeConfiguration<BundleDeal>
    {
        public void Configure(EntityTypeBuilder<BundleDeal> builder)
        {
            builder.ToTable("bundle_deals");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(b => b.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            builder.Property(b => b.BuyProductId).HasColumnName("buy_product_id");
            builder.Property(b => b.BuyQuantity).HasColumnName("buy_quantity");
            builder.Property(b => b.GetProductId).HasColumnName("get_product_id");
            builder.Property(b => b.GetQuantity).HasColumnName("get_quantity");
            builder.Property(b => b.GetDiscountPercent).HasColumnName("get_discount_percent").HasColumnType("decimal(5,2)");
            builder.Property(b => b.IsActive).HasColumnName("is_active");
            builder.Property(b => b.StartsAt).HasColumnName("starts_at");
            builder.Property(b => b.EndsAt).HasColumnName("ends_at");

            builder.HasOne(b => b.BuyProduct).WithMany().HasForeignKey(b => b.BuyProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(b => b.GetProduct).WithMany().HasForeignKey(b => b.GetProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
