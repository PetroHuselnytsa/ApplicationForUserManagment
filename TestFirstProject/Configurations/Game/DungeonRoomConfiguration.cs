using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class DungeonRoomConfiguration : IEntityTypeConfiguration<DungeonRoom>
    {
        public void Configure(EntityTypeBuilder<DungeonRoom> builder)
        {
            builder.ToTable("game_dungeon_rooms");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(r => r.DungeonRunId).HasColumnName("dungeon_run_id");
            builder.Property(r => r.RoomIndex).HasColumnName("room_index");
            builder.Property(r => r.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.EnemyId).HasColumnName("enemy_id");
            builder.Property(r => r.TreasureItemId).HasColumnName("treasure_item_id");
            builder.Property(r => r.TrapDamage).HasColumnName("trap_damage").HasDefaultValue(0);
            builder.Property(r => r.RestorePercent).HasColumnName("restore_percent").HasDefaultValue(0);
            builder.Property(r => r.BattleId).HasColumnName("battle_id");

            builder.HasOne(r => r.DungeonRun)
                   .WithMany(dr => dr.Rooms)
                   .HasForeignKey(r => r.DungeonRunId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Enemy)
                   .WithMany()
                   .HasForeignKey(r => r.EnemyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.TreasureItem)
                   .WithMany()
                   .HasForeignKey(r => r.TreasureItemId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.Battle)
                   .WithMany()
                   .HasForeignKey(r => r.BattleId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(r => r.DungeonRunId);
            builder.HasIndex(r => new { r.DungeonRunId, r.RoomIndex }).IsUnique();
        }
    }
}
