using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class LearnedSkillConfiguration : IEntityTypeConfiguration<LearnedSkill>
    {
        public void Configure(EntityTypeBuilder<LearnedSkill> builder)
        {
            builder.ToTable("game_learned_skills");

            builder.HasKey(ls => ls.Id);

            builder.Property(ls => ls.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(ls => ls.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(ls => ls.SkillId)
                   .HasColumnName("skill_id");

            builder.Property(ls => ls.LearnedAt)
                   .HasColumnName("learned_at");

            // Relationships
            builder.HasOne(ls => ls.Character)
                   .WithMany(c => c.LearnedSkills)
                   .HasForeignKey(ls => ls.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ls => ls.Skill)
                   .WithMany(s => s.LearnedBy)
                   .HasForeignKey(ls => ls.SkillId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique index to prevent learning the same skill twice
            builder.HasIndex(ls => new { ls.CharacterId, ls.SkillId }).IsUnique();
        }
    }
}
