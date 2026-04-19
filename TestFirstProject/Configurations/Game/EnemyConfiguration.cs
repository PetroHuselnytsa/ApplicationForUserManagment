using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class EnemyConfiguration : IEntityTypeConfiguration<Enemy>
    {
        public void Configure(EntityTypeBuilder<Enemy> builder)
        {
            builder.ToTable("game_enemies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnName("id");

            builder.Property(e => e.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.ZoneId)
                   .HasColumnName("zone_id");

            builder.Property(e => e.IsBoss)
                   .HasColumnName("is_boss");

            builder.Property(e => e.BossMechanic)
                   .HasColumnName("boss_mechanic")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(e => e.DamageType)
                   .HasColumnName("damage_type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(e => e.BaseHP)
                   .HasColumnName("base_hp");

            builder.Property(e => e.BaseMP)
                   .HasColumnName("base_mp");

            builder.Property(e => e.BaseAttack)
                   .HasColumnName("base_attack");

            builder.Property(e => e.BaseDefense)
                   .HasColumnName("base_defense");

            builder.Property(e => e.BaseMagicPower)
                   .HasColumnName("base_magic_power");

            builder.Property(e => e.BaseSpeed)
                   .HasColumnName("base_speed");

            builder.Property(e => e.BaseCritChance)
                   .HasColumnName("base_crit_chance");

            builder.Property(e => e.BaseDodgeChance)
                   .HasColumnName("base_dodge_chance");

            builder.Property(e => e.ExperienceReward)
                   .HasColumnName("experience_reward");

            builder.Property(e => e.GoldReward)
                   .HasColumnName("gold_reward");

            builder.Property(e => e.AIStrategy)
                   .HasColumnName("ai_strategy")
                   .HasMaxLength(50);

            builder.HasOne(e => e.Zone)
                   .WithMany(z => z.Enemies)
                   .HasForeignKey(e => e.ZoneId);

            builder.HasMany(e => e.LootTable)
                   .WithOne(l => l.Enemy)
                   .HasForeignKey(l => l.EnemyId);

            builder.HasMany(e => e.Skills)
                   .WithOne(s => s.Enemy)
                   .HasForeignKey(s => s.EnemyId);

            builder.HasIndex(e => e.ZoneId);
        }
    }
}
