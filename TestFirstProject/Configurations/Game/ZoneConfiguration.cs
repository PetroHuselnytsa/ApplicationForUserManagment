using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("game_zones");

            builder.HasKey(z => z.Id);

            builder.Property(z => z.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(z => z.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(z => z.Description).HasColumnName("description").IsRequired().HasMaxLength(500);
            builder.Property(z => z.MinLevel).HasColumnName("min_level");
            builder.Property(z => z.MaxLevel).HasColumnName("max_level");

            builder.HasIndex(z => z.Name).IsUnique();
        }
    }
}
