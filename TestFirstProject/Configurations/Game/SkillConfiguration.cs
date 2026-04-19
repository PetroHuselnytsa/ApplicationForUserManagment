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

            builder.Property(s => s.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(s => s.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.Description)
                   .HasColumnName("description")
                   .HasMaxLength(500);

            builder.Property(s => s.Class)
                   .HasColumnName("class")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(s => s.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(s => s.DamageType)
                   .HasColumnName("damage_type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(s => s.TargetType)
                   .HasColumnName("target_type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(s => s.UnlockLevel)
                   .HasColumnName("unlock_level");

            builder.Property(s => s.ManaCost)
                   .HasColumnName("mana_cost");

            builder.Property(s => s.Cooldown)
                   .HasColumnName("cooldown");

            builder.Property(s => s.DamageMultiplier)
                   .HasColumnName("damage_multiplier");

            builder.Property(s => s.AppliesEffect)
                   .HasColumnName("applies_effect")
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(s => s.EffectDuration)
                   .HasColumnName("effect_duration");

            builder.Property(s => s.EffectTickValue)
                   .HasColumnName("effect_tick_value");

            builder.Property(s => s.PassiveBonusStat)
                   .HasColumnName("passive_bonus_stat")
                   .HasMaxLength(50);

            builder.Property(s => s.PassiveBonusValue)
                   .HasColumnName("passive_bonus_value");

            // Indexes
            builder.HasIndex(s => new { s.Class, s.UnlockLevel });
        }
    }
}
