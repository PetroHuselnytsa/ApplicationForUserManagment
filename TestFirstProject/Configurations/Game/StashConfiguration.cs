using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class StashConfiguration : IEntityTypeConfiguration<Stash>
    {
        public void Configure(EntityTypeBuilder<Stash> builder)
        {
            builder.ToTable("game_stash");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(s => s.PlayerId).HasColumnName("player_id");
            builder.Property(s => s.ItemId).HasColumnName("item_id");
            builder.Property(s => s.Quantity).HasColumnName("quantity").HasDefaultValue(1);

            builder.HasOne(s => s.Item)
                   .WithMany()
                   .HasForeignKey(s => s.ItemId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.PlayerId);
            builder.HasIndex(s => new { s.PlayerId, s.ItemId });
        }
    }
}
