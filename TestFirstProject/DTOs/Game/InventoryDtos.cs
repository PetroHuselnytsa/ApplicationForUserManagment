namespace TestFirstProject.DTOs.Game
{
    // --- Inventory DTOs ---

    public record InventoryItemDto(
        Guid Id,
        Guid ItemId,
        string Name,
        string Description,
        string Type,
        string Rarity,
        int Quantity,
        int EnchantmentLevel,
        int LevelRequirement,
        string? ClassRestriction,
        ItemStatsDto Stats
    );

    public record ItemStatsDto(
        int BonusHp,
        int BonusMp,
        int BonusAttack,
        int BonusDefense,
        int BonusMagicPower,
        int BonusSpeed,
        double BonusCritChance,
        double BonusDodgeChance,
        int EnchantBonusAttack,
        int EnchantBonusDefense,
        int EnchantBonusHp,
        int EnchantBonusMagicPower
    );

    public record EquipItemRequest(
        Guid InventoryItemId
    );

    public record UseItemRequest(
        Guid InventoryItemId
    );

    public record EnchantItemRequest(
        Guid InventoryItemId
    );

    public record ConsumableUseResultDto(
        int HpRestored,
        int MpRestored,
        string? BuffApplied,
        string Message
    );

    public record StashItemDto(
        Guid Id,
        Guid ItemId,
        string Name,
        string Type,
        string Rarity,
        int Quantity
    );

    public record MoveToStashRequest(
        Guid InventoryItemId,
        int Quantity
    );

    public record MoveFromStashRequest(
        Guid StashId,
        Guid CharacterId,
        int Quantity
    );
}
