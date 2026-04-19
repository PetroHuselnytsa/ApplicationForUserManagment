using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("game_items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(i => i.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(i => i.Description).HasColumnName("description").IsRequired().HasMaxLength(500);
            builder.Property(i => i.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(i => i.Rarity).HasColumnName("rarity").HasConversion<string>().HasMaxLength(20);
            builder.Property(i => i.LevelRequirement).HasColumnName("level_requirement").HasDefaultValue(1);
            builder.Property(i => i.ClassRestriction).HasColumnName("class_restriction").HasConversion<string?>().HasMaxLength(20);
            builder.Property(i => i.BonusHp).HasColumnName("bonus_hp").HasDefaultValue(0);
            builder.Property(i => i.BonusMp).HasColumnName("bonus_mp").HasDefaultValue(0);
            builder.Property(i => i.BonusAttack).HasColumnName("bonus_attack").HasDefaultValue(0);
            builder.Property(i => i.BonusDefense).HasColumnName("bonus_defense").HasDefaultValue(0);
            builder.Property(i => i.BonusMagicPower).HasColumnName("bonus_magic_power").HasDefaultValue(0);
            builder.Property(i => i.BonusSpeed).HasColumnName("bonus_speed").HasDefaultValue(0);
            builder.Property(i => i.BonusCritChance).HasColumnName("bonus_crit_chance").HasDefaultValue(0.0);
            builder.Property(i => i.BonusDodgeChance).HasColumnName("bonus_dodge_chance").HasDefaultValue(0.0);
            builder.Property(i => i.ConsumableType).HasColumnName("consumable_type").HasConversion<string?>().HasMaxLength(20);
            builder.Property(i => i.HealAmount).HasColumnName("heal_amount").HasDefaultValue(0);
            builder.Property(i => i.ManaAmount).HasColumnName("mana_amount").HasDefaultValue(0);
            builder.Property(i => i.BuffEffect).HasColumnName("buff_effect").HasConversion<string?>().HasMaxLength(20);
            builder.Property(i => i.BuffDuration).HasColumnName("buff_duration").HasDefaultValue(0);
            builder.Property(i => i.BuyPrice).HasColumnName("buy_price").HasDefaultValue(0);
            builder.Property(i => i.SellPrice).HasColumnName("sell_price").HasDefaultValue(0);

            builder.HasIndex(i => i.Type);
            builder.HasIndex(i => i.Rarity);
        }
    }
}
