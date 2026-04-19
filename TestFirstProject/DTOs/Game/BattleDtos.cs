namespace TestFirstProject.DTOs.Game
{
    // --- Battle DTOs ---

    public record StartBattleRequest(
        Guid CharacterId,
        Guid EnemyId
    );

    public record BattleActionRequest(
        string ActionType,
        Guid? SkillId,
        Guid? ItemId,
        Guid? TargetId
    );

    public record BattleStateDto(
        Guid Id,
        string Status,
        int CurrentTurn,
        List<BattleParticipantDto> Participants,
        List<ActiveStatusEffectDto> StatusEffects
    );

    public record BattleParticipantDto(
        Guid Id,
        string Name,
        string Type,
        int CurrentHp,
        int MaxHp,
        int CurrentMp,
        int MaxMp,
        int Attack,
        int Defense,
        int Speed,
        bool IsAlive,
        bool IsPhaseTwo
    );

    public record ActiveStatusEffectDto(
        string Type,
        Guid TargetParticipantId,
        string TargetName,
        int RemainingTurns,
        int TickValue,
        int Stacks
    );

    public record BattleTurnResultDto(
        Guid BattleId,
        string Status,
        int TurnNumber,
        List<BattleLogEntryDto> Log,
        List<DroppedItemDto>? Loot,
        int XpAwarded,
        int GoldAwarded
    );

    public record BattleLogEntryDto(
        string Actor,
        string Action,
        string? Target,
        int? Damage,
        int? Healing,
        bool IsCritical,
        bool IsDodged,
        string? StatusEffect,
        string? Message
    );

    public record DroppedItemDto(
        Guid ItemId,
        string ItemName,
        int Quantity
    );
}
