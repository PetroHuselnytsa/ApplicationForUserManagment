using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class BattleParticipantConfiguration : IEntityTypeConfiguration<BattleParticipant>
    {
        public void Configure(EntityTypeBuilder<BattleParticipant> builder)
        {
            builder.ToTable("game_battle_participants");

            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(bp => bp.BattleId)
                   .HasColumnName("battle_id");

            builder.Property(bp => bp.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(bp => bp.EnemyId)
                   .HasColumnName("enemy_id");

            builder.Property(bp => bp.IsPlayer)
                   .HasColumnName("is_player");

            builder.Property(bp => bp.Name)
                   .HasColumnName("name")
                   .IsRequired();

            builder.Property(bp => bp.CurrentHP)
                   .HasColumnName("current_hp");

            builder.Property(bp => bp.MaxHP)
                   .HasColumnName("max_hp");

            builder.Property(bp => bp.CurrentMP)
                   .HasColumnName("current_mp");

            builder.Property(bp => bp.MaxMP)
                   .HasColumnName("max_mp");

            builder.Property(bp => bp.Attack)
                   .HasColumnName("attack");

            builder.Property(bp => bp.Defense)
                   .HasColumnName("defense");

            builder.Property(bp => bp.MagicPower)
                   .HasColumnName("magic_power");

            builder.Property(bp => bp.Speed)
                   .HasColumnName("speed");

            builder.Property(bp => bp.CritChance)
                   .HasColumnName("crit_chance");

            builder.Property(bp => bp.DodgeChance)
                   .HasColumnName("dodge_chance");

            builder.Property(bp => bp.IsAlive)
                   .HasColumnName("is_alive");

            builder.Property(bp => bp.IsPhase2)
                   .HasColumnName("is_phase2");

            // Relationships
            builder.HasOne(bp => bp.Battle)
                   .WithMany(b => b.Participants)
                   .HasForeignKey(bp => bp.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bp => bp.Character)
                   .WithMany()
                   .HasForeignKey(bp => bp.CharacterId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(bp => bp.Enemy)
                   .WithMany()
                   .HasForeignKey(bp => bp.EnemyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(bp => bp.StatusEffects)
                   .WithOne(se => se.BattleParticipant)
                   .HasForeignKey(se => se.BattleParticipantId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(bp => bp.BattleId);
        }
    }
}
