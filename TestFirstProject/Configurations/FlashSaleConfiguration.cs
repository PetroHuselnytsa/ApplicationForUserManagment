using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class FlashSaleConfiguration : IEntityTypeConfiguration<FlashSale>
    {
        public void Configure(EntityTypeBuilder<FlashSale> builder)
        {
            builder.ToTable("flash_sales");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(f => f.ProductId).HasColumnName("product_id");
            builder.Property(f => f.SalePrice).HasColumnName("sale_price").HasColumnType("decimal(18,2)");
            builder.Property(f => f.StartsAt).HasColumnName("starts_at");
            builder.Property(f => f.EndsAt).HasColumnName("ends_at");
            builder.Property(f => f.IsActive).HasColumnName("is_active");
            builder.Property(f => f.CreatedAt).HasColumnName("created_at");

            builder.HasOne(f => f.Product).WithMany(p => p.FlashSales).HasForeignKey(f => f.ProductId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(f => f.ProductId);
            builder.HasIndex(f => new { f.StartsAt, f.EndsAt });
        }
    }
}
