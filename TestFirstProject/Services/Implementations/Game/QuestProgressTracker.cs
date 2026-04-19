using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Tracks quest progress, handles acceptance/completion, and manages quest chains.
    /// </summary>
    public class QuestProgressTracker : IQuestProgressTracker
    {
        private readonly GameDbContext _db;
        private readonly ICharacterProgressionService _progression;
        private readonly ILootService _lootService;

        public QuestProgressTracker(GameDbContext db, ICharacterProgressionService progression, ILootService lootService)
        {
            _db = db;
            _progression = progression;
            _lootService = lootService;
        }

        public async Task<CharacterQuestsResult> GetQuestsAsync(Guid characterId)
        {
            var character = await _db.Characters.FindAsync(characterId)
                ?? throw new NotFoundException("Character not found.");

            // Get all character quests
            var characterQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest)
                .Where(cq => cq.CharacterId == characterId)
                .ToListAsync();

            var activeQuests = characterQuests.Where(cq => cq.Status == QuestStatus.Active).ToList();
            var completedQuests = characterQuests.Where(cq => cq.Status == QuestStatus.Completed).ToList();
            var acceptedQuestIds = characterQuests.Select(cq => cq.QuestId).ToHashSet();

            // Find available quests (not yet accepted, prerequisites met, level met)
            var completedQuestIds = completedQuests.Select(cq => cq.QuestId).ToHashSet();

            var allQuests = await _db.Quests.ToListAsync();
            var availableQuests = allQuests
                .Where(q => !acceptedQuestIds.Contains(q.Id))
                .Where(q => character.Level >= q.MinLevelRequirement)
                .Where(q => !q.PrerequisiteQuestId.HasValue || completedQuestIds.Contains(q.PrerequisiteQuestId.Value))
                .ToList();

            return new CharacterQuestsResult
            {
                ActiveQuests = activeQuests,
                AvailableQuests = availableQuests,
                CompletedQuests = completedQuests
            };
        }

        public async Task<CharacterQuest> AcceptQuestAsync(Guid characterId, Guid questId)
        {
            var character = await _db.Characters.FindAsync(characterId)
                ?? throw new NotFoundException("Character not found.");

            var quest = await _db.Quests.FindAsync(questId)
                ?? throw new NotFoundException("Quest not found.");

            // Check if already accepted
            var existing = await _db.CharacterQuests
                .FirstOrDefaultAsync(cq => cq.CharacterId == characterId && cq.QuestId == questId);
            if (existing != null)
                throw new ConflictException("Quest already accepted.");

            // Check level requirement
            if (character.Level < quest.MinLevelRequirement)
                throw new ValidationException($"Must be level {quest.MinLevelRequirement} to accept this quest.");

            // Check prerequisite
            if (quest.PrerequisiteQuestId.HasValue)
            {
                var prereqComplete = await _db.CharacterQuests
                    .AnyAsync(cq => cq.CharacterId == characterId
                        && cq.QuestId == quest.PrerequisiteQuestId.Value
                        && cq.Status == QuestStatus.Completed);

                if (!prereqComplete)
                    throw new ValidationException("Prerequisite quest not completed.");
            }

            var characterQuest = new CharacterQuest
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                QuestId = questId,
                Status = QuestStatus.Active,
                CurrentCount = 0,
                AcceptedAt = DateTime.UtcNow,
                Deadline = quest.TimeLimitMinutes > 0
                    ? DateTime.UtcNow.AddMinutes(quest.TimeLimitMinutes)
                    : null
            };

            // For ReachLevel quests, check if already at required level
            if (quest.Type == QuestType.ReachLevel && quest.TargetLevel.HasValue)
            {
                if (character.Level >= quest.TargetLevel.Value)
                    characterQuest.CurrentCount = 1;
            }

            _db.CharacterQuests.Add(characterQuest);
            await _db.SaveChangesAsync();

            return characterQuest;
        }

        public async Task<QuestCompletionResult> CompleteQuestAsync(Guid characterId, Guid questId)
        {
            var characterQuest = await _db.CharacterQuests
                .Include(cq => cq.Quest).ThenInclude(q => q.RewardItem)
                .FirstOrDefaultAsync(cq => cq.CharacterId == characterId && cq.QuestId == questId)
                ?? throw new NotFoundException("Quest not found for this character.");

            if (characterQuest.Status != QuestStatus.Active)
                throw new ValidationException("Quest is not active.");

            var quest = characterQuest.Quest;

            // Check time limit
            if (characterQuest.Deadline.HasValue && DateTime.UtcNow > characterQuest.Deadline.Value)
            {
                characterQuest.Status = QuestStatus.Failed;
                await _db.SaveChangesAsync();
                return new QuestCompletionResult
                {
                    Success = false,
                    Message = "Quest deadline has passed. Quest failed."
                };
            }

            // Validate completion requirements
            bool isComplete = quest.Type switch
            {
                QuestType.KillCount => characterQuest.CurrentCount >= quest.RequiredCount,
                QuestType.CollectItem => characterQuest.CurrentCount >= quest.RequiredCount,
                QuestType.BossDefeat => characterQuest.CurrentCount >= quest.RequiredCount,
                QuestType.ReachLevel => characterQuest.CurrentCount >= 1,
                _ => false
            };

            if (!isComplete)
            {
                return new QuestCompletionResult
                {
                    Success = false,
                    Message = $"Quest requirements not met. Progress: {characterQuest.CurrentCount}/{quest.RequiredCount}"
                };
            }

            // Complete quest
            characterQuest.Status = QuestStatus.Completed;
            characterQuest.CompletedAt = DateTime.UtcNow;

            // Award rewards
            var character = await _db.Characters.FindAsync(characterId)!;
            character!.Gold += quest.GoldReward;
            await _progression.AwardExperienceAsync(characterId, quest.XpReward);

            var result = new QuestCompletionResult
            {
                Success = true,
                Message = $"Quest '{quest.Name}' completed!",
                XpReward = quest.XpReward,
                GoldReward = quest.GoldReward,
                NextQuestId = quest.NextQuestId
            };

            // Award item reward
            if (quest.RewardItemId.HasValue)
            {
                await _lootService.AddItemsToInventoryAsync(characterId, new List<DroppedItem>
                {
                    new() { ItemId = quest.RewardItemId.Value, ItemName = quest.RewardItem?.Name ?? "Reward", Quantity = quest.RewardItemQuantity }
                });
                result.RewardItemName = quest.RewardItem?.Name;
            }

            await _db.SaveChangesAsync();
            return result;
        }

        public async Task OnEnemyKilledAsync(Guid characterId, Guid enemyId)
        {
            // Update kill count quests
            var relevantQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest)
                .Where(cq => cq.CharacterId == characterId
                    && cq.Status == QuestStatus.Active
                    && (cq.Quest.Type == QuestType.KillCount || cq.Quest.Type == QuestType.BossDefeat)
                    && cq.Quest.TargetEnemyId == enemyId)
                .ToListAsync();

            foreach (var cq in relevantQuests)
            {
                cq.CurrentCount++;
            }

            await _db.SaveChangesAsync();
        }

        public async Task OnItemCollectedAsync(Guid characterId, Guid itemId, int quantity)
        {
            var relevantQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest)
                .Where(cq => cq.CharacterId == characterId
                    && cq.Status == QuestStatus.Active
                    && cq.Quest.Type == QuestType.CollectItem
                    && cq.Quest.TargetItemId == itemId)
                .ToListAsync();

            foreach (var cq in relevantQuests)
            {
                cq.CurrentCount += quantity;
            }

            await _db.SaveChangesAsync();
        }

        public async Task OnLevelUpAsync(Guid characterId, int newLevel)
        {
            var relevantQuests = await _db.CharacterQuests
                .Include(cq => cq.Quest)
                .Where(cq => cq.CharacterId == characterId
                    && cq.Status == QuestStatus.Active
                    && cq.Quest.Type == QuestType.ReachLevel
                    && cq.Quest.TargetLevel <= newLevel)
                .ToListAsync();

            foreach (var cq in relevantQuests)
            {
                cq.CurrentCount = 1; // Boolean-like: reached the level
            }

            await _db.SaveChangesAsync();
        }
    }
}
