using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class DungeonRunConfiguration : IEntityTypeConfiguration<DungeonRun>
    {
        public void Configure(EntityTypeBuilder<DungeonRun> builder)
        {
            builder.ToTable("game_dungeon_runs");

            builder.HasKey(dr => dr.Id);

            builder.Property(dr => dr.Id)
                   .HasColumnName("id");

            builder.Property(dr => dr.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(dr => dr.ZoneId)
                   .HasColumnName("zone_id");

            builder.Property(dr => dr.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(dr => dr.CurrentRoomIndex)
                   .HasColumnName("current_room_index");

            builder.Property(dr => dr.StartedAt)
                   .HasColumnName("started_at");

            builder.Property(dr => dr.CompletedAt)
                   .HasColumnName("completed_at");

            builder.HasOne(dr => dr.Character)
                   .WithMany()
                   .HasForeignKey(dr => dr.CharacterId);

            builder.HasOne(dr => dr.Zone)
                   .WithMany(z => z.DungeonRuns)
                   .HasForeignKey(dr => dr.ZoneId);

            builder.HasMany(dr => dr.Rooms)
                   .WithOne(r => r.DungeonRun)
                   .HasForeignKey(r => r.DungeonRunId);

            builder.HasIndex(dr => dr.CharacterId);
            builder.HasIndex(dr => dr.Status);
        }
    }
}
