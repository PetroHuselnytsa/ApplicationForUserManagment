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

            builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(b => b.CharacterId).HasColumnName("character_id");
            builder.Property(b => b.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.CurrentTurn).HasColumnName("current_turn").HasDefaultValue(1);
            builder.Property(b => b.CurrentTurnIndex).HasColumnName("current_turn_index").HasDefaultValue(0);
            builder.Property(b => b.DungeonRoomId).HasColumnName("dungeon_room_id");
            builder.Property(b => b.XpReward).HasColumnName("xp_reward").HasDefaultValue(0);
            builder.Property(b => b.GoldReward).HasColumnName("gold_reward").HasDefaultValue(0);
            builder.Property(b => b.CreatedAt).HasColumnName("created_at");
            builder.Property(b => b.CompletedAt).HasColumnName("completed_at");

            // Optimistic concurrency
            builder.Property(b => b.RowVersion)
                   .HasColumnName("row_version")
                   .IsRowVersion();

            builder.HasOne(b => b.Character)
                   .WithMany()
                   .HasForeignKey(b => b.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => b.CharacterId);
            builder.HasIndex(b => new { b.CharacterId, b.Status });
        }
    }
}
