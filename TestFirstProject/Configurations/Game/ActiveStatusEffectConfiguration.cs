using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class ActiveStatusEffectConfiguration : IEntityTypeConfiguration<ActiveStatusEffect>
    {
        public void Configure(EntityTypeBuilder<ActiveStatusEffect> builder)
        {
            builder.ToTable("game_active_status_effects");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(e => e.BattleId).HasColumnName("battle_id");
            builder.Property(e => e.TargetParticipantId).HasColumnName("target_participant_id");
            builder.Property(e => e.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.RemainingTurns).HasColumnName("remaining_turns");
            builder.Property(e => e.TickValue).HasColumnName("tick_value");
            builder.Property(e => e.Stacks).HasColumnName("stacks").HasDefaultValue(1);

            builder.HasOne(e => e.Battle)
                   .WithMany(b => b.StatusEffects)
                   .HasForeignKey(e => e.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.TargetParticipant)
                   .WithMany()
                   .HasForeignKey(e => e.TargetParticipantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.BattleId);
            builder.HasIndex(e => e.TargetParticipantId);
        }
    }
}
