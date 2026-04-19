using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("game_items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id)
                   .HasColumnName("id");

            builder.Property(i => i.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);

            builder.Property(i => i.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(i => i.Rarity)
                   .HasColumnName("rarity")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(i => i.LevelRequirement)
                   .HasColumnName("level_requirement");

            builder.Property(i => i.ClassRestriction)
                   .HasColumnName("class_restriction")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(i => i.BonusHP)
                   .HasColumnName("bonus_hp");

            builder.Property(i => i.BonusMP)
                   .HasColumnName("bonus_mp");

            builder.Property(i => i.BonusAttack)
                   .HasColumnName("bonus_attack");

            builder.Property(i => i.BonusDefense)
                   .HasColumnName("bonus_defense");

            builder.Property(i => i.BonusMagicPower)
                   .HasColumnName("bonus_magic_power");

            builder.Property(i => i.BonusSpeed)
                   .HasColumnName("bonus_speed");

            builder.Property(i => i.BonusCritChance)
                   .HasColumnName("bonus_crit_chance");

            builder.Property(i => i.BonusDodgeChance)
                   .HasColumnName("bonus_dodge_chance");

            builder.Property(i => i.ConsumableType)
                   .HasColumnName("consumable_type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(i => i.ConsumableValue)
                   .HasColumnName("consumable_value");

            builder.Property(i => i.ScrollEffect)
                   .HasColumnName("scroll_effect")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(i => i.ScrollEffectDuration)
                   .HasColumnName("scroll_effect_duration");

            builder.Property(i => i.GoldValue)
                   .HasColumnName("gold_value");

            builder.HasIndex(i => i.Type);
            builder.HasIndex(i => i.Rarity);
        }
    }
}
