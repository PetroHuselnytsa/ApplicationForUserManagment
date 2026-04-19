using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class DungeonRoomConfiguration : IEntityTypeConfiguration<DungeonRoom>
    {
        public void Configure(EntityTypeBuilder<DungeonRoom> builder)
        {
            builder.ToTable("game_dungeon_rooms");

            builder.HasKey(dr => dr.Id);

            builder.Property(dr => dr.Id)
                   .HasColumnName("id");

            builder.Property(dr => dr.DungeonRunId)
                   .HasColumnName("dungeon_run_id");

            builder.Property(dr => dr.RoomIndex)
                   .HasColumnName("room_index");

            builder.Property(dr => dr.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(dr => dr.IsCompleted)
                   .HasColumnName("is_completed");

            builder.Property(dr => dr.EnemyId)
                   .HasColumnName("enemy_id");

            builder.Property(dr => dr.TreasureItemId)
                   .HasColumnName("treasure_item_id");

            builder.Property(dr => dr.TrapDamage)
                   .HasColumnName("trap_damage");

            builder.Property(dr => dr.RestHealPercent)
                   .HasColumnName("rest_heal_percent");

            builder.Property(dr => dr.BattleId)
                   .HasColumnName("battle_id");

            builder.HasOne(dr => dr.DungeonRun)
                   .WithMany(d => d.Rooms)
                   .HasForeignKey(dr => dr.DungeonRunId);

            builder.HasOne(dr => dr.Enemy)
                   .WithMany()
                   .HasForeignKey(dr => dr.EnemyId);

            builder.HasOne(dr => dr.TreasureItem)
                   .WithMany()
                   .HasForeignKey(dr => dr.TreasureItemId);

            builder.HasOne(dr => dr.Battle)
                   .WithMany()
                   .HasForeignKey(dr => dr.BattleId);

            builder.HasIndex(dr => dr.DungeonRunId);
        }
    }
}
