using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Quest management: accept, track progress, complete, and chain quests.
    /// </summary>
    public interface IQuestProgressTracker
    {
        /// <summary>Get all active and available quests for a character.</summary>
        Task<List<QuestResponse>> GetActiveQuestsAsync(Guid playerId, Guid characterId);

        /// <summary>Get available quests that the character can accept.</summary>
        Task<List<AvailableQuestResponse>> GetAvailableQuestsAsync(Guid playerId, Guid characterId);

        /// <summary>Accept a quest for a character.</summary>
        Task<QuestResponse> AcceptQuestAsync(Guid playerId, Guid characterId, Guid questId);

        /// <summary>Complete a quest and award rewards.</summary>
        Task<QuestResponse> CompleteQuestAsync(Guid playerId, Guid characterId, Guid questId);

        /// <summary>Track enemy kills for active KillCount quests.</summary>
        Task TrackEnemyKillAsync(Guid characterId, string enemyName);

        /// <summary>Track item collection for active CollectItem quests.</summary>
        Task TrackItemCollectedAsync(Guid characterId, Guid itemId);

        /// <summary>Track boss defeats for active BossDefeat quests.</summary>
        Task TrackBossDefeatAsync(Guid characterId, string bossName);
    }
}
