using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Quest management: accept, track progress through combat/loot hooks, complete, and chain quests.
    /// </summary>
    public class QuestProgressTracker : IQuestProgressTracker
    {
        private readonly PersonsContext _db;

        public QuestProgressTracker(PersonsContext db)
        {
            _db = db;
        }

        public async Task<List<QuestResponse>> GetActiveQuestsAsync(Guid playerId, Guid characterId)
        {
            await VerifyCharacterOwnership(playerId, characterId);

            var activeQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest).ThenInclude(q => q.RewardItem)
                .Where(cq => cq.CharacterId == characterId &&
                            (cq.Status == QuestStatus.Active || cq.Status == QuestStatus.Completed))
                .OrderBy(cq => cq.AcceptedAt)
                .ToListAsync();

            return activeQuests.Select(MapToQuestResponse).ToList();
        }

        public async Task<List<AvailableQuestResponse>> GetAvailableQuestsAsync(Guid playerId, Guid characterId)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            // Single query: load (QuestId, Status) pairs and partition in memory
            var characterQuests = await _db.CharacterQuests
                .Where(cq => cq.CharacterId == characterId)
                .Select(cq => new { cq.QuestId, cq.Status })
                .ToListAsync();

            var takenQuestIds = characterQuests.Select(cq => cq.QuestId).ToHashSet();
            var completedQuestIds = characterQuests
                .Where(cq => cq.Status == QuestStatus.Completed)
                .Select(cq => cq.QuestId)
                .ToHashSet();

            var availableQuests = await _db.Quests
                .Include(q => q.RewardItem)
                .Where(q => !takenQuestIds.Contains(q.Id) &&
                           q.MinLevel <= character.Level &&
                           (!q.PrerequisiteQuestId.HasValue || completedQuestIds.Contains(q.PrerequisiteQuestId.Value)))
                .OrderBy(q => q.MinLevel)
                .ToListAsync();

            return availableQuests.Select(q => new AvailableQuestResponse(
                q.Id, q.Name, q.Description, q.Type.ToString(),
                q.RequiredCount, q.RewardXP, q.RewardGold,
                q.RewardItem?.Name, q.MinLevel, q.TimeLimitMinutes
            )).ToList();
        }

        public async Task<QuestResponse> AcceptQuestAsync(Guid playerId, Guid characterId, Guid questId)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            var quest = await _db.Quests
                .Include(q => q.RewardItem)
                .FirstOrDefaultAsync(q => q.Id == questId)
                ?? throw new NotFoundException("Quest not found.");

            var existing = await _db.CharacterQuests
                .AnyAsync(cq => cq.CharacterId == characterId && cq.QuestId == questId);
            if (existing)
                throw new ConflictException("Quest already accepted.");

            if (quest.MinLevel > character.Level)
                throw new ValidationException($"Character must be level {quest.MinLevel} to accept this quest.");

            if (quest.PrerequisiteQuestId.HasValue)
            {
                var prereqCompleted = await _db.CharacterQuests
                    .AnyAsync(cq => cq.CharacterId == characterId &&
                                   cq.QuestId == quest.PrerequisiteQuestId.Value &&
                                   cq.Status == QuestStatus.Completed);
                if (!prereqCompleted)
                    throw new ValidationException("Prerequisite quest has not been completed.");
            }

            var initialProgress = 0;
            if (quest.Type == QuestType.ReachLevel && quest.TargetLevel.HasValue)
            {
                initialProgress = character.Level >= quest.TargetLevel.Value ? quest.RequiredCount : character.Level;
            }

            var characterQuest = new CharacterQuest
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                QuestId = questId,
                Status = QuestStatus.Active,
                Progress = initialProgress,
                AcceptedAt = DateTime.UtcNow
            };

            _db.CharacterQuests.Add(characterQuest);
            await _db.SaveChangesAsync();

            return MapToQuestResponse(characterQuest);
        }

        public async Task<QuestResponse> CompleteQuestAsync(Guid playerId, Guid characterId, Guid questId)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            var charQuest = await _db.CharacterQuests
                .Include(cq => cq.Quest).ThenInclude(q => q.RewardItem)
                .FirstOrDefaultAsync(cq => cq.CharacterId == characterId && cq.QuestId == questId)
                ?? throw new NotFoundException("Quest not found for this character.");

            if (charQuest.Status != QuestStatus.Active)
                throw new ValidationException($"Quest status is {charQuest.Status}, not Active.");

            if (charQuest.Progress < charQuest.Quest.RequiredCount)
                throw new ValidationException($"Quest not complete. Progress: {charQuest.Progress}/{charQuest.Quest.RequiredCount}");

            if (charQuest.Quest.TimeLimitMinutes.HasValue)
            {
                var elapsed = (DateTime.UtcNow - charQuest.AcceptedAt).TotalMinutes;
                if (elapsed > charQuest.Quest.TimeLimitMinutes.Value)
                {
                    charQuest.Status = QuestStatus.Failed;
                    await _db.SaveChangesAsync();
                    throw new ValidationException("Quest time limit has expired.");
                }
            }

            charQuest.Status = QuestStatus.Completed;
            charQuest.CompletedAt = DateTime.UtcNow;

            character.Experience += charQuest.Quest.RewardXP;
            character.Gold += charQuest.Quest.RewardGold;

            if (charQuest.Quest.RewardItemId.HasValue)
            {
                await InventoryHelper.AddOrIncrementAsync(_db, characterId, charQuest.Quest.RewardItemId.Value);
            }

            await _db.SaveChangesAsync();

            return MapToQuestResponse(charQuest);
        }

        public async Task TrackEnemyKillAsync(Guid characterId, string enemyName)
        {
            await IncrementQuestProgressAsync(characterId, QuestType.KillCount,
                q => q.TargetName == enemyName);
        }

        public async Task TrackItemCollectedAsync(Guid characterId, Guid itemId)
        {
            await IncrementQuestProgressAsync(characterId, QuestType.CollectItem,
                q => q.TargetItemId == itemId);
        }

        public async Task TrackBossDefeatAsync(Guid characterId, string bossName)
        {
            await IncrementQuestProgressAsync(characterId, QuestType.BossDefeat,
                q => q.TargetName == bossName);
        }

        #region Private Helpers

        /// <summary>
        /// Unified quest progress tracker: loads matching active quests and increments progress.
        /// Does NOT call SaveChangesAsync — the caller (CombatEngine) saves in a single batch.
        /// </summary>
        private async Task IncrementQuestProgressAsync(
            Guid characterId,
            QuestType questType,
            Expression<Func<Quest, bool>> targetFilter)
        {
            // Build the combined filter: active quests of the right type + target match
            var matchingQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest)
                .Where(cq => cq.CharacterId == characterId &&
                            cq.Status == QuestStatus.Active &&
                            cq.Quest.Type == questType)
                .ToListAsync();

            // Apply the target filter in memory (Expression can't be combined with the EF query cleanly
            // for all cases, and the result set is small — typically 0-2 matching quests)
            var compiledFilter = targetFilter.Compile();
            foreach (var quest in matchingQuests.Where(cq => compiledFilter(cq.Quest)))
            {
                quest.Progress++;
            }
        }

        private async Task VerifyCharacterOwnership(Guid playerId, Guid characterId)
        {
            var exists = await _db.Characters
                .AnyAsync(c => c.Id == characterId && c.PlayerId == playerId);
            if (!exists)
                throw new NotFoundException("Character not found.");
        }

        private static QuestResponse MapToQuestResponse(CharacterQuest cq)
        {
            return new QuestResponse(
                cq.QuestId, cq.Quest.Name, cq.Quest.Description,
                cq.Quest.Type.ToString(), cq.Status.ToString(),
                cq.Progress, cq.Quest.RequiredCount,
                cq.Quest.RewardXP, cq.Quest.RewardGold,
                cq.Quest.RewardItem?.Name,
                cq.AcceptedAt, cq.CompletedAt
            );
        }

        #endregion
    }
}
