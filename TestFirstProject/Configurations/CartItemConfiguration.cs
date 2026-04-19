using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("cart_items");

            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(ci => ci.CartId).HasColumnName("cart_id");
            builder.Property(ci => ci.ProductVariantId).HasColumnName("product_variant_id");
            builder.Property(ci => ci.Quantity).HasColumnName("quantity");
            builder.Property(ci => ci.AddedAt).HasColumnName("added_at");

            builder.HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ci => ci.ProductVariant).WithMany().HasForeignKey(ci => ci.ProductVariantId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ci => new { ci.CartId, ci.ProductVariantId }).IsUnique();
        }
    }
}
