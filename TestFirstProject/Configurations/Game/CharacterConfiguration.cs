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
                   .HasColumnName("level");

            builder.Property(c => c.Experience)
                   .HasColumnName("experience");

            builder.Property(c => c.Gold)
                   .HasColumnName("gold");

            builder.Property(c => c.SkillPoints)
                   .HasColumnName("skill_points");

            builder.Property(c => c.CreatedAt)
                   .HasColumnName("created_at");

            builder.Property(c => c.RowVersion)
                   .HasColumnName("row_version")
                   .IsRowVersion();

            // Relationships
            builder.HasOne(c => c.Player)
                   .WithMany()
                   .HasForeignKey(c => c.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.LearnedSkills)
                   .WithOne(ls => ls.Character)
                   .HasForeignKey(ls => ls.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Inventory)
                   .WithOne(i => i.Character)
                   .HasForeignKey(i => i.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.EquippedItems)
                   .WithOne(e => e.Character)
                   .HasForeignKey(e => e.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Quests)
                   .WithOne(q => q.Character)
                   .HasForeignKey(q => q.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.PlayerId);
        }
    }
}
