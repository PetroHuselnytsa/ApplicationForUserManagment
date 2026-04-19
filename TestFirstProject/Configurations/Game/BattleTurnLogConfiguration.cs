using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class BattleTurnLogConfiguration : IEntityTypeConfiguration<BattleTurnLog>
    {
        public void Configure(EntityTypeBuilder<BattleTurnLog> builder)
        {
            builder.ToTable("game_battle_turn_logs");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(t => t.BattleId)
                   .HasColumnName("battle_id");

            builder.Property(t => t.TurnNumber)
                   .HasColumnName("turn_number");

            builder.Property(t => t.ActorId)
                   .HasColumnName("actor_id");

            builder.Property(t => t.TargetId)
                   .HasColumnName("target_id");

            builder.Property(t => t.Action)
                   .HasColumnName("action")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(t => t.ActionDetail)
                   .HasColumnName("action_detail")
                   .HasMaxLength(100);

            builder.Property(t => t.DamageDealt)
                   .HasColumnName("damage_dealt");

            builder.Property(t => t.HealingDone)
                   .HasColumnName("healing_done");

            builder.Property(t => t.WasCritical)
                   .HasColumnName("was_critical");

            builder.Property(t => t.WasDodged)
                   .HasColumnName("was_dodged");

            builder.Property(t => t.StatusEffectApplied)
                   .HasColumnName("status_effect_applied")
                   .HasMaxLength(50);

            builder.Property(t => t.Description)
                   .HasColumnName("description")
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(t => t.Timestamp)
                   .HasColumnName("timestamp");

            // Relationships
            builder.HasOne(t => t.Battle)
                   .WithMany(b => b.TurnLogs)
                   .HasForeignKey(t => t.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => new { t.BattleId, t.TurnNumber });
        }
    }
}
