using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class QuestConfiguration : IEntityTypeConfiguration<Quest>
    {
        public void Configure(EntityTypeBuilder<Quest> builder)
        {
            builder.ToTable("game_quests");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Id)
                   .HasColumnName("id");

            builder.Property(q => q.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(q => q.Description)
                   .HasColumnName("description")
                   .HasMaxLength(1000);

            builder.Property(q => q.Type)
                   .HasColumnName("type")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(q => q.ZoneId)
                   .HasColumnName("zone_id");

            builder.Property(q => q.RequiredCount)
                   .HasColumnName("required_count");

            builder.Property(q => q.TargetName)
                   .HasColumnName("target_name")
                   .HasMaxLength(100);

            builder.Property(q => q.TargetItemId)
                   .HasColumnName("target_item_id");

            builder.Property(q => q.TargetLevel)
                   .HasColumnName("target_level");

            builder.Property(q => q.PrerequisiteQuestId)
                   .HasColumnName("prerequisite_quest_id");

            builder.Property(q => q.TimeLimitMinutes)
                   .HasColumnName("time_limit_minutes");

            builder.Property(q => q.RewardXP)
                   .HasColumnName("reward_xp");

            builder.Property(q => q.RewardGold)
                   .HasColumnName("reward_gold");

            builder.Property(q => q.RewardItemId)
                   .HasColumnName("reward_item_id");

            builder.Property(q => q.MinLevel)
                   .HasColumnName("min_level");

            builder.HasOne(q => q.Zone)
                   .WithMany(z => z.Quests)
                   .HasForeignKey(q => q.ZoneId);

            builder.HasOne(q => q.PrerequisiteQuest)
                   .WithMany()
                   .HasForeignKey(q => q.PrerequisiteQuestId);

            builder.HasOne(q => q.RewardItem)
                   .WithMany()
                   .HasForeignKey(q => q.RewardItemId);

            builder.HasOne(q => q.TargetItem)
                   .WithMany()
                   .HasForeignKey(q => q.TargetItemId);

            builder.HasMany(q => q.CharacterQuests)
                   .WithOne(cq => cq.Quest)
                   .HasForeignKey(cq => cq.QuestId);

            builder.HasIndex(q => q.ZoneId);
            builder.HasIndex(q => q.Type);
        }
    }
}
