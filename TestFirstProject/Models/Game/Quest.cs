using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Quest definition with objectives, rewards, and chain links.
    /// </summary>
    public class Quest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public QuestType Type { get; set; }

        // Objective details
        public int RequiredCount { get; set; } = 1;
        public Guid? TargetEnemyId { get; set; }
        public Guid? TargetItemId { get; set; }
        public int? TargetLevel { get; set; }

        // Prerequisites
        public Guid? PrerequisiteQuestId { get; set; }
        public Quest? PrerequisiteQuest { get; set; }
        public int MinLevelRequirement { get; set; } = 1;

        // Rewards
        public int XpReward { get; set; }
        public int GoldReward { get; set; }
        public Guid? RewardItemId { get; set; }
        public Item? RewardItem { get; set; }
        public int RewardItemQuantity { get; set; } = 1;

        // Optional time limit (in minutes, 0 = no limit)
        public int TimeLimitMinutes { get; set; } = 0;

        // Zone association
        public Guid? ZoneId { get; set; }
        public Zone? Zone { get; set; }

        // Quest chain
        public Guid? NextQuestId { get; set; }
        public Quest? NextQuest { get; set; }
    }
}
