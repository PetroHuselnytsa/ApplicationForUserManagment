using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Tracks quest progress, handles acceptance and completion, and manages quest chains.
    /// </summary>
    public interface IQuestProgressTracker
    {
        /// <summary>Get all quests for a character (active, available, completed).</summary>
        Task<CharacterQuestsResult> GetQuestsAsync(Guid characterId);

        /// <summary>Accept an available quest.</summary>
        Task<CharacterQuest> AcceptQuestAsync(Guid characterId, Guid questId);

        /// <summary>Try to complete a quest, validating requirements are met.</summary>
        Task<QuestCompletionResult> CompleteQuestAsync(Guid characterId, Guid questId);

        /// <summary>Record progress toward kill-count quests after an enemy is defeated.</summary>
        Task OnEnemyKilledAsync(Guid characterId, Guid enemyId);

        /// <summary>Record progress toward collect-item quests when an item is acquired.</summary>
        Task OnItemCollectedAsync(Guid characterId, Guid itemId, int quantity);

        /// <summary>Check reach-level quests when character levels up.</summary>
        Task OnLevelUpAsync(Guid characterId, int newLevel);
    }

    public class CharacterQuestsResult
    {
        public List<CharacterQuest> ActiveQuests { get; set; } = new();
        public List<Quest> AvailableQuests { get; set; } = new();
        public List<CharacterQuest> CompletedQuests { get; set; } = new();
    }

    public class QuestCompletionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int XpReward { get; set; }
        public int GoldReward { get; set; }
        public string? RewardItemName { get; set; }
        public Guid? NextQuestId { get; set; }
    }
}
