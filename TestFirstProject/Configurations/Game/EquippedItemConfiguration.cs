using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class EquippedItemConfiguration : IEntityTypeConfiguration<EquippedItem>
    {
        public void Configure(EntityTypeBuilder<EquippedItem> builder)
        {
            builder.ToTable("game_equipped_items");

            builder.HasKey(ei => ei.Id);

            builder.Property(ei => ei.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(ei => ei.CharacterId).HasColumnName("character_id");
            builder.Property(ei => ei.InventoryItemId).HasColumnName("inventory_item_id");
            builder.Property(ei => ei.Slot).HasColumnName("slot").HasConversion<string>().HasMaxLength(20);

            builder.HasOne(ei => ei.Character)
                   .WithMany(c => c.EquippedItems)
                   .HasForeignKey(ei => ei.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ei => ei.InventoryItem)
                   .WithMany()
                   .HasForeignKey(ei => ei.InventoryItemId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One item per slot per character
            builder.HasIndex(ei => new { ei.CharacterId, ei.Slot }).IsUnique();
        }
    }
}
