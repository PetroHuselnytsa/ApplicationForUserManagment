using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class QuestConfiguration : IEntityTypeConfiguration<Quest>
    {
        public void Configure(EntityTypeBuilder<Quest> builder)
        {
            builder.ToTable("game_quests");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(q => q.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            builder.Property(q => q.Description).HasColumnName("description").IsRequired().HasMaxLength(1000);
            builder.Property(q => q.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(q => q.RequiredCount).HasColumnName("required_count").HasDefaultValue(1);
            builder.Property(q => q.TargetEnemyId).HasColumnName("target_enemy_id");
            builder.Property(q => q.TargetItemId).HasColumnName("target_item_id");
            builder.Property(q => q.TargetLevel).HasColumnName("target_level");
            builder.Property(q => q.PrerequisiteQuestId).HasColumnName("prerequisite_quest_id");
            builder.Property(q => q.MinLevelRequirement).HasColumnName("min_level_requirement").HasDefaultValue(1);
            builder.Property(q => q.XpReward).HasColumnName("xp_reward");
            builder.Property(q => q.GoldReward).HasColumnName("gold_reward");
            builder.Property(q => q.RewardItemId).HasColumnName("reward_item_id");
            builder.Property(q => q.RewardItemQuantity).HasColumnName("reward_item_quantity").HasDefaultValue(1);
            builder.Property(q => q.TimeLimitMinutes).HasColumnName("time_limit_minutes").HasDefaultValue(0);
            builder.Property(q => q.ZoneId).HasColumnName("zone_id");
            builder.Property(q => q.NextQuestId).HasColumnName("next_quest_id");

            builder.HasOne(q => q.PrerequisiteQuest)
                   .WithMany()
                   .HasForeignKey(q => q.PrerequisiteQuestId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(q => q.RewardItem)
                   .WithMany()
                   .HasForeignKey(q => q.RewardItemId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(q => q.Zone)
                   .WithMany(z => z.Quests)
                   .HasForeignKey(q => q.ZoneId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(q => q.NextQuest)
                   .WithMany()
                   .HasForeignKey(q => q.NextQuestId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(q => q.ZoneId);
            builder.HasIndex(q => q.Type);
        }
    }
}
