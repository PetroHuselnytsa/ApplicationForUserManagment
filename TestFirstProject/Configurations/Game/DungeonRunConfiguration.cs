using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class DungeonRunConfiguration : IEntityTypeConfiguration<DungeonRun>
    {
        public void Configure(EntityTypeBuilder<DungeonRun> builder)
        {
            builder.ToTable("game_dungeon_runs");

            builder.HasKey(dr => dr.Id);

            builder.Property(dr => dr.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(dr => dr.CharacterId).HasColumnName("character_id");
            builder.Property(dr => dr.ZoneId).HasColumnName("zone_id");
            builder.Property(dr => dr.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(dr => dr.CurrentRoomIndex).HasColumnName("current_room_index").HasDefaultValue(0);
            builder.Property(dr => dr.StartedAt).HasColumnName("started_at");
            builder.Property(dr => dr.CompletedAt).HasColumnName("completed_at");

            builder.HasOne(dr => dr.Character)
                   .WithMany()
                   .HasForeignKey(dr => dr.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dr => dr.Zone)
                   .WithMany()
                   .HasForeignKey(dr => dr.ZoneId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(dr => dr.CharacterId);
            builder.HasIndex(dr => new { dr.CharacterId, dr.Status });
        }
    }
}
