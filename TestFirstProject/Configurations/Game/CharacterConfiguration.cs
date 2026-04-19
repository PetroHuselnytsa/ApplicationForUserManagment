using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.ToTable("game_characters");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(c => c.PlayerId)
                   .HasColumnName("player_id");

            builder.Property(c => c.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Class)
                   .HasColumnName("class")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(c => c.Level)
                   .HasColumnName("level")
                   .HasDefaultValue(1);

            builder.Property(c => c.Experience)
                   .HasColumnName("experience")
                   .HasDefaultValue(0);

            builder.Property(c => c.SkillPoints)
                   .HasColumnName("skill_points")
                   .HasDefaultValue(0);

            builder.Property(c => c.CurrentHp)
                   .HasColumnName("current_hp");

            builder.Property(c => c.CurrentMp)
                   .HasColumnName("current_mp");

            builder.Property(c => c.Gold)
                   .HasColumnName("gold")
                   .HasDefaultValue(0);

            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at");

            // Player relationship (AppUser from existing schema)
            builder.HasIndex(c => c.PlayerId);
            builder.HasIndex(c => new { c.PlayerId, c.Name }).IsUnique();
        }
    }
}
