using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(i => i.CartId)
            .HasColumnName("cart_id")
            .IsRequired();

        builder.Property(i => i.ProductVariantId)
            .HasColumnName("product_variant_id")
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // One entry per variant per cart (update quantity instead of duplicating)
        builder.HasIndex(i => new { i.CartId, i.ProductVariantId }).IsUnique();
    }
}
