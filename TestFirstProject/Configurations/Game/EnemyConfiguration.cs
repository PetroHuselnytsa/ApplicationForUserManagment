using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class EnemyConfiguration : IEntityTypeConfiguration<Enemy>
    {
        public void Configure(EntityTypeBuilder<Enemy> builder)
        {
            builder.ToTable("game_enemies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(e => e.IsBoss).HasColumnName("is_boss").HasDefaultValue(false);
            builder.Property(e => e.BossMechanic).HasColumnName("boss_mechanic").HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.BaseHp).HasColumnName("base_hp");
            builder.Property(e => e.BaseMp).HasColumnName("base_mp");
            builder.Property(e => e.BaseAttack).HasColumnName("base_attack");
            builder.Property(e => e.BaseDefense).HasColumnName("base_defense");
            builder.Property(e => e.BaseMagicPower).HasColumnName("base_magic_power");
            builder.Property(e => e.BaseSpeed).HasColumnName("base_speed");
            builder.Property(e => e.BaseCritChance).HasColumnName("base_crit_chance");
            builder.Property(e => e.BaseDodgeChance).HasColumnName("base_dodge_chance");
            builder.Property(e => e.PrimaryDamageType).HasColumnName("primary_damage_type").HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.BaseXpReward).HasColumnName("base_xp_reward");
            builder.Property(e => e.BaseGoldReward).HasColumnName("base_gold_reward");
            builder.Property(e => e.ZoneId).HasColumnName("zone_id");

            builder.HasOne(e => e.Zone)
                   .WithMany(z => z.Enemies)
                   .HasForeignKey(e => e.ZoneId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Many-to-many with skills via a join table
            builder.HasMany(e => e.Skills)
                   .WithMany()
                   .UsingEntity(j => j.ToTable("game_enemy_skills"));

            builder.HasIndex(e => e.ZoneId);
            builder.HasIndex(e => e.IsBoss);
        }
    }
}
