namespace TestFirstProject.DTOs.Game
{
    // --- Quest DTOs ---

    public record CharacterQuestsDto(
        List<ActiveQuestDto> ActiveQuests,
        List<AvailableQuestDto> AvailableQuests,
        List<CompletedQuestDto> CompletedQuests
    );

    public record ActiveQuestDto(
        Guid QuestId,
        string Name,
        string Description,
        string Type,
        int CurrentCount,
        int RequiredCount,
        DateTime AcceptedAt,
        DateTime? Deadline
    );

    public record AvailableQuestDto(
        Guid Id,
        string Name,
        string Description,
        string Type,
        int RequiredCount,
        int MinLevelRequirement,
        int XpReward,
        int GoldReward,
        string? RewardItemName
    );

    public record CompletedQuestDto(
        Guid QuestId,
        string Name,
        DateTime? CompletedAt
    );

    public record QuestCompletionResultDto(
        bool Success,
        string Message,
        int XpReward,
        int GoldReward,
        string? RewardItemName,
        Guid? NextQuestId
    );

    public record ZoneDto(
        Guid Id,
        string Name,
        string Description,
        int MinLevel,
        int MaxLevel,
        List<ZoneEnemyDto> Enemies
    );

    public record ZoneEnemyDto(
        Guid Id,
        string Name,
        bool IsBoss,
        string DamageType
    );
}
