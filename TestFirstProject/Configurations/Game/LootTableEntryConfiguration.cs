using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class LootTableEntryConfiguration : IEntityTypeConfiguration<LootTableEntry>
    {
        public void Configure(EntityTypeBuilder<LootTableEntry> builder)
        {
            builder.ToTable("game_loot_table_entries");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                   .HasColumnName("id");

            builder.Property(l => l.EnemyId)
                   .HasColumnName("enemy_id");

            builder.Property(l => l.ItemId)
                   .HasColumnName("item_id");

            builder.Property(l => l.DropChance)
                   .HasColumnName("drop_chance");

            builder.Property(l => l.MinQuantity)
                   .HasColumnName("min_quantity");

            builder.Property(l => l.MaxQuantity)
                   .HasColumnName("max_quantity");

            builder.HasOne(l => l.Enemy)
                   .WithMany(e => e.LootTable)
                   .HasForeignKey(l => l.EnemyId);

            builder.HasOne(l => l.Item)
                   .WithMany(i => i.LootEntries)
                   .HasForeignKey(l => l.ItemId);

            builder.HasIndex(l => l.EnemyId);
        }
    }
}
