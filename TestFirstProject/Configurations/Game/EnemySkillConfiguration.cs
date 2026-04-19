using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class EnemySkillConfiguration : IEntityTypeConfiguration<EnemySkill>
    {
        public void Configure(EntityTypeBuilder<EnemySkill> builder)
        {
            builder.ToTable("game_enemy_skills");

            builder.HasKey(es => es.Id);

            builder.Property(es => es.Id)
                   .HasColumnName("id");

            builder.Property(es => es.EnemyId)
                   .HasColumnName("enemy_id");

            builder.Property(es => es.SkillId)
                   .HasColumnName("skill_id");

            builder.HasOne(es => es.Enemy)
                   .WithMany(e => e.Skills)
                   .HasForeignKey(es => es.EnemyId);

            builder.HasOne(es => es.Skill)
                   .WithMany(s => s.EnemySkills)
                   .HasForeignKey(es => es.SkillId);

            builder.HasIndex(es => new { es.EnemyId, es.SkillId })
                   .IsUnique();
        }
    }
}
