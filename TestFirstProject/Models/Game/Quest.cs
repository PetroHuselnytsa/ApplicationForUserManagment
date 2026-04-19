using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Quest definition (seeded data).
    /// </summary>
    public class Quest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public QuestType Type { get; set; }
        public Guid? ZoneId { get; set; }

        /// <summary>Target count for KillCount/CollectItem quests.</summary>
        public int RequiredCount { get; set; }

        /// <summary>Target enemy name for KillCount or BossDefeat quests.</summary>
        public string? TargetName { get; set; }

        /// <summary>Target item ID for CollectItem quests.</summary>
        public Guid? TargetItemId { get; set; }

        /// <summary>Target level for ReachLevel quests.</summary>
        public int? TargetLevel { get; set; }

        /// <summary>Prerequisite quest ID (for quest chains).</summary>
        public Guid? PrerequisiteQuestId { get; set; }

        /// <summary>Optional time limit in minutes. Null means no time limit.</summary>
        public int? TimeLimitMinutes { get; set; }

        // Rewards
        public int RewardXP { get; set; }
        public int RewardGold { get; set; }

        /// <summary>Optional item reward.</summary>
        public Guid? RewardItemId { get; set; }

        /// <summary>Minimum level to accept this quest.</summary>
        public int MinLevel { get; set; } = 1;

        // Navigation
        public Zone? Zone { get; set; }
        public Quest? PrerequisiteQuest { get; set; }
        public Item? RewardItem { get; set; }
        public Item? TargetItem { get; set; }
        public ICollection<CharacterQuest> CharacterQuests { get; set; } = new List<CharacterQuest>();
    }
}
