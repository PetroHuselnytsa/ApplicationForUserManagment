using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class CharacterStatsConfiguration : IEntityTypeConfiguration<CharacterStats>
    {
        public void Configure(EntityTypeBuilder<CharacterStats> builder)
        {
            builder.ToTable("game_character_stats");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(s => s.CharacterId).HasColumnName("character_id");
            builder.Property(s => s.BaseHp).HasColumnName("base_hp");
            builder.Property(s => s.BaseMp).HasColumnName("base_mp");
            builder.Property(s => s.BaseAttack).HasColumnName("base_attack");
            builder.Property(s => s.BaseDefense).HasColumnName("base_defense");
            builder.Property(s => s.BaseMagicPower).HasColumnName("base_magic_power");
            builder.Property(s => s.BaseSpeed).HasColumnName("base_speed");
            builder.Property(s => s.BaseCritChance).HasColumnName("base_crit_chance");
            builder.Property(s => s.BaseDodgeChance).HasColumnName("base_dodge_chance");
            builder.Property(s => s.BonusHp).HasColumnName("bonus_hp").HasDefaultValue(0);
            builder.Property(s => s.BonusMp).HasColumnName("bonus_mp").HasDefaultValue(0);
            builder.Property(s => s.BonusAttack).HasColumnName("bonus_attack").HasDefaultValue(0);
            builder.Property(s => s.BonusDefense).HasColumnName("bonus_defense").HasDefaultValue(0);
            builder.Property(s => s.BonusMagicPower).HasColumnName("bonus_magic_power").HasDefaultValue(0);
            builder.Property(s => s.BonusSpeed).HasColumnName("bonus_speed").HasDefaultValue(0);

            // Ignore computed properties
            builder.Ignore(s => s.TotalHp);
            builder.Ignore(s => s.TotalMp);
            builder.Ignore(s => s.TotalAttack);
            builder.Ignore(s => s.TotalDefense);
            builder.Ignore(s => s.TotalMagicPower);
            builder.Ignore(s => s.TotalSpeed);

            // One-to-one with Character
            builder.HasOne(s => s.Character)
                   .WithOne(c => c.Stats)
                   .HasForeignKey<CharacterStats>(s => s.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.CharacterId).IsUnique();
        }
    }
}
