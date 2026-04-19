using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("game_inventory_items");

            builder.HasKey(ii => ii.Id);

            builder.Property(ii => ii.Id)
                   .HasColumnName("id");

            builder.Property(ii => ii.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(ii => ii.ItemId)
                   .HasColumnName("item_id");

            builder.Property(ii => ii.Quantity)
                   .HasColumnName("quantity");

            builder.Property(ii => ii.EnchantmentStat)
                   .HasColumnName("enchantment_stat")
                   .HasMaxLength(50);

            builder.Property(ii => ii.EnchantmentValue)
                   .HasColumnName("enchantment_value");

            builder.HasOne(ii => ii.Character)
                   .WithMany()
                   .HasForeignKey(ii => ii.CharacterId);

            builder.HasOne(ii => ii.Item)
                   .WithMany(i => i.InventoryItems)
                   .HasForeignKey(ii => ii.ItemId);

            builder.HasIndex(ii => ii.CharacterId);
        }
    }
}
