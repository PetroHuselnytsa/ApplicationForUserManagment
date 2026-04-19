using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class BattleConfiguration : IEntityTypeConfiguration<Battle>
    {
        public void Configure(EntityTypeBuilder<Battle> builder)
        {
            builder.ToTable("game_battles");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(b => b.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(b => b.DungeonRoomId)
                   .HasColumnName("dungeon_room_id");

            builder.Property(b => b.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(b => b.CurrentTurn)
                   .HasColumnName("current_turn");

            builder.Property(b => b.CurrentTurnParticipantId)
                   .HasColumnName("current_turn_participant_id");

            builder.Property(b => b.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(b => b.CompletedAt)
                   .HasColumnName("completed_at");

            builder.Property(b => b.RowVersion)
                   .HasColumnName("row_version")
                   .IsRowVersion();

            // Relationships
            builder.HasOne(b => b.Character)
                   .WithMany()
                   .HasForeignKey(b => b.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.DungeonRoom)
                   .WithMany()
                   .HasForeignKey(b => b.DungeonRoomId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(b => b.Participants)
                   .WithOne(p => p.Battle)
                   .HasForeignKey(p => p.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.TurnLogs)
                   .WithOne(t => t.Battle)
                   .HasForeignKey(t => t.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(b => b.CharacterId);
            builder.HasIndex(b => b.Status);
        }
    }
}
