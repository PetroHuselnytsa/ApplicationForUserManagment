using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class LootTableEntryConfiguration : IEntityTypeConfiguration<LootTableEntry>
    {
        public void Configure(EntityTypeBuilder<LootTableEntry> builder)
        {
            builder.ToTable("game_loot_table");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(l => l.EnemyId).HasColumnName("enemy_id");
            builder.Property(l => l.ItemId).HasColumnName("item_id");
            builder.Property(l => l.DropChance).HasColumnName("drop_chance");
            builder.Property(l => l.MinQuantity).HasColumnName("min_quantity").HasDefaultValue(1);
            builder.Property(l => l.MaxQuantity).HasColumnName("max_quantity").HasDefaultValue(1);

            builder.HasOne(l => l.Enemy)
                   .WithMany(e => e.LootTable)
                   .HasForeignKey(l => l.EnemyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.Item)
                   .WithMany()
                   .HasForeignKey(l => l.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => l.EnemyId);
        }
    }
}
