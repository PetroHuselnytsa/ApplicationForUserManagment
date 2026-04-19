using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class SkillConfiguration : IEntityTypeConfiguration<Skill>
    {
        public void Configure(EntityTypeBuilder<Skill> builder)
        {
            builder.ToTable("game_skills");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(s => s.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(s => s.Description).HasColumnName("description").IsRequired().HasMaxLength(500);
            builder.Property(s => s.Class).HasColumnName("class").HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.UnlockLevel).HasColumnName("unlock_level");
            builder.Property(s => s.MpCost).HasColumnName("mp_cost");
            builder.Property(s => s.CooldownTurns).HasColumnName("cooldown_turns");
            builder.Property(s => s.TargetType).HasColumnName("target_type").HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.DamageType).HasColumnName("damage_type").HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.BaseDamage).HasColumnName("base_damage");
            builder.Property(s => s.DamageMultiplier).HasColumnName("damage_multiplier").HasDefaultValue(1.0);
            builder.Property(s => s.PassiveHpBonus).HasColumnName("passive_hp_bonus").HasDefaultValue(0);
            builder.Property(s => s.PassiveMpBonus).HasColumnName("passive_mp_bonus").HasDefaultValue(0);
            builder.Property(s => s.PassiveAttackBonus).HasColumnName("passive_attack_bonus").HasDefaultValue(0);
            builder.Property(s => s.PassiveDefenseBonus).HasColumnName("passive_defense_bonus").HasDefaultValue(0);
            builder.Property(s => s.PassiveMagicPowerBonus).HasColumnName("passive_magic_power_bonus").HasDefaultValue(0);
            builder.Property(s => s.PassiveSpeedBonus).HasColumnName("passive_speed_bonus").HasDefaultValue(0);
            builder.Property(s => s.AppliesEffect).HasColumnName("applies_effect").HasConversion<string?>().HasMaxLength(20);
            builder.Property(s => s.EffectDuration).HasColumnName("effect_duration");
            builder.Property(s => s.EffectTickValue).HasColumnName("effect_tick_value");
            builder.Property(s => s.EffectStackLimit).HasColumnName("effect_stack_limit").HasDefaultValue(1);

            builder.HasIndex(s => s.Class);
            builder.HasIndex(s => new { s.Class, s.UnlockLevel });
        }
    }
}
