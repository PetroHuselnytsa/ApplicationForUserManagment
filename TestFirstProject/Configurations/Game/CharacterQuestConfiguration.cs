using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class CharacterQuestConfiguration : IEntityTypeConfiguration<CharacterQuest>
    {
        public void Configure(EntityTypeBuilder<CharacterQuest> builder)
        {
            builder.ToTable("game_character_quests");

            builder.HasKey(cq => cq.Id);

            builder.Property(cq => cq.Id)
                   .HasColumnName("id");

            builder.Property(cq => cq.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(cq => cq.QuestId)
                   .HasColumnName("quest_id");

            builder.Property(cq => cq.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(cq => cq.Progress)
                   .HasColumnName("progress");

            builder.Property(cq => cq.AcceptedAt)
                   .HasColumnName("accepted_at");

            builder.Property(cq => cq.CompletedAt)
                   .HasColumnName("completed_at");

            builder.HasOne(cq => cq.Character)
                   .WithMany()
                   .HasForeignKey(cq => cq.CharacterId);

            builder.HasOne(cq => cq.Quest)
                   .WithMany(q => q.CharacterQuests)
                   .HasForeignKey(cq => cq.QuestId);

            builder.HasIndex(cq => new { cq.CharacterId, cq.QuestId })
                   .IsUnique();
        }
    }
}
