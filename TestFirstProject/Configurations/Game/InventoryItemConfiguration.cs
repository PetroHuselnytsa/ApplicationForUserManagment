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

            builder.Property(ii => ii.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(ii => ii.CharacterId).HasColumnName("character_id");
            builder.Property(ii => ii.ItemId).HasColumnName("item_id");
            builder.Property(ii => ii.Quantity).HasColumnName("quantity").HasDefaultValue(1);
            builder.Property(ii => ii.EnchantmentLevel).HasColumnName("enchantment_level").HasDefaultValue(0);
            builder.Property(ii => ii.EnchantBonusAttack).HasColumnName("enchant_bonus_attack").HasDefaultValue(0);
            builder.Property(ii => ii.EnchantBonusDefense).HasColumnName("enchant_bonus_defense").HasDefaultValue(0);
            builder.Property(ii => ii.EnchantBonusHp).HasColumnName("enchant_bonus_hp").HasDefaultValue(0);
            builder.Property(ii => ii.EnchantBonusMagicPower).HasColumnName("enchant_bonus_magic_power").HasDefaultValue(0);

            builder.HasOne(ii => ii.Character)
                   .WithMany(c => c.Inventory)
                   .HasForeignKey(ii => ii.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ii => ii.Item)
                   .WithMany()
                   .HasForeignKey(ii => ii.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ii => ii.CharacterId);
            builder.HasIndex(ii => new { ii.CharacterId, ii.ItemId });
        }
    }
}
