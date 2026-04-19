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

            builder.Property(e => e.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(e => e.BattleParticipantId)
                   .HasColumnName("battle_participant_id");

            builder.Property(e => e.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(e => e.RemainingTurns)
                   .HasColumnName("remaining_turns");

            builder.Property(e => e.TickValue)
                   .HasColumnName("tick_value");

            builder.Property(e => e.StackCount)
                   .HasColumnName("stack_count");

            // Relationships
            builder.HasOne(e => e.BattleParticipant)
                   .WithMany(bp => bp.StatusEffects)
                   .HasForeignKey(e => e.BattleParticipantId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.BattleParticipantId);
        }
    }
}
