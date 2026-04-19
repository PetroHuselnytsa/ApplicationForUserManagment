namespace TestFirstProject.DTOs.Game
{
    // --- Character DTOs ---

    public record CreateCharacterRequest(string Name, string Class);

    public record AllocateStatsRequest(int HP, int MP, int Attack, int Defense, int MagicPower, int Speed);

    public record CharacterResponse(Guid Id, string Name, string Class, int Level, int Experience, int Gold, int SkillPoints, DateTime CreatedAt);

    public record CharacterStatsResponse(Guid CharacterId, string Name, string Class, int Level, int HP, int MaxHP, int MP, int MaxMP, int Attack, int Defense, int MagicPower, int Speed, double CritChance, double DodgeChance, List<EquippedItemResponse> Equipment, List<LearnedSkillResponse> Skills);

    public record LearnedSkillResponse(Guid SkillId, string Name, string Description, string Type, string TargetType, int ManaCost, int Cooldown);

    // --- Battle DTOs ---

    public record StartBattleRequest(Guid CharacterId, Guid? EnemyId, Guid? ZoneId);

    public record BattleActionRequest(string Action, Guid? SkillId, Guid? ItemId, Guid? TargetId);

    public record BattleStateResponse(Guid BattleId, string Status, int CurrentTurn, Guid? CurrentTurnParticipantId, List<BattleParticipantResponse> Participants, List<BattleTurnLogResponse> RecentLogs);

    public record BattleParticipantResponse(Guid Id, string Name, bool IsPlayer, int CurrentHP, int MaxHP, int CurrentMP, int MaxMP, bool IsAlive, List<StatusEffectResponse> StatusEffects);

    public record StatusEffectResponse(string Type, int RemainingTurns, int TickValue, int StackCount);

    public record BattleTurnLogResponse(int TurnNumber, string ActorName, string? TargetName, string Action, string? ActionDetail, int DamageDealt, int HealingDone, bool WasCritical, bool WasDodged, string Description);

    public record BattleResultResponse(string Status, int XPAwarded, int GoldAwarded, List<LootDropResponse> Loot);

    public record LootDropResponse(string ItemName, string Rarity, int Quantity);

    // --- Inventory DTOs ---

    public record InventoryItemResponse(Guid Id, Guid ItemId, string Name, string Description, string Type, string Rarity, int Quantity, string? EnchantmentStat, int EnchantmentValue);

    public record EquipItemRequest(Guid InventoryItemId, string Slot);

    public record EquippedItemResponse(string Slot, string ItemName, string Rarity, Guid InventoryItemId);

    public record UseItemRequest(Guid InventoryItemId);

    public record EnchantItemRequest(Guid InventoryItemId, Guid MaterialItemId);

    public record StashItemResponse(Guid Id, Guid ItemId, string Name, string Type, string Rarity, int Quantity);

    // --- Dungeon DTOs ---

    public record DungeonRunResponse(Guid Id, string ZoneName, string Status, int CurrentRoomIndex, int TotalRooms, List<DungeonRoomResponse> Rooms);

    public record DungeonRoomResponse(int RoomIndex, string Type, bool IsCompleted, string? EnemyName, Guid? BattleId);

    public record DungeonActionRequest(string Action);

    public record StartDungeonRequest(Guid CharacterId);

    // --- Quest DTOs ---

    public record QuestResponse(Guid Id, string Name, string Description, string Type, string Status, int Progress, int RequiredCount, int RewardXP, int RewardGold, string? RewardItemName, DateTime? AcceptedAt, DateTime? CompletedAt);

    public record AvailableQuestResponse(Guid Id, string Name, string Description, string Type, int RequiredCount, int RewardXP, int RewardGold, string? RewardItemName, int MinLevel, int? TimeLimitMinutes);
}
