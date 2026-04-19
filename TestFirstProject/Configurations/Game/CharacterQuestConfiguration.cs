using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class CharacterQuestConfiguration : IEntityTypeConfiguration<CharacterQuest>
    {
        public void Configure(EntityTypeBuilder<CharacterQuest> builder)
        {
            builder.ToTable("game_character_quests");

            builder.HasKey(cq => cq.Id);

            builder.Property(cq => cq.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(cq => cq.CharacterId).HasColumnName("character_id");
            builder.Property(cq => cq.QuestId).HasColumnName("quest_id");
            builder.Property(cq => cq.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(cq => cq.CurrentCount).HasColumnName("current_count").HasDefaultValue(0);
            builder.Property(cq => cq.AcceptedAt).HasColumnName("accepted_at");
            builder.Property(cq => cq.CompletedAt).HasColumnName("completed_at");
            builder.Property(cq => cq.Deadline).HasColumnName("deadline");

            builder.HasOne(cq => cq.Character)
                   .WithMany(c => c.Quests)
                   .HasForeignKey(cq => cq.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cq => cq.Quest)
                   .WithMany()
                   .HasForeignKey(cq => cq.QuestId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cq => cq.CharacterId);
            builder.HasIndex(cq => new { cq.CharacterId, cq.QuestId }).IsUnique();
            builder.HasIndex(cq => new { cq.CharacterId, cq.Status });
        }
    }
}
