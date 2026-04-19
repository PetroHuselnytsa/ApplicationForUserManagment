namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Handles loot rolling, item distribution, inventory management, equipment, and enchanting.
    /// </summary>
    public interface ILootService
    {
        /// <summary>Roll loot drops for a defeated enemy.</summary>
        Task<List<DroppedItem>> RollLootAsync(Guid enemyId);

        /// <summary>Add dropped items to a character's inventory.</summary>
        Task AddItemsToInventoryAsync(Guid characterId, List<DroppedItem> items);

        /// <summary>Get a character's inventory.</summary>
        Task<List<Models.Game.InventoryItem>> GetInventoryAsync(Guid characterId);

        /// <summary>Equip an inventory item.</summary>
        Task EquipItemAsync(Guid characterId, Guid inventoryItemId);

        /// <summary>Unequip an item from a slot.</summary>
        Task UnequipItemAsync(Guid characterId, Models.Game.Enums.ItemType slot);

        /// <summary>Use a consumable item. Returns true if used in battle context.</summary>
        Task<ConsumableUseResult> UseConsumableAsync(Guid characterId, Guid inventoryItemId, Guid? battleId = null);

        /// <summary>Enchant an equipment item using materials.</summary>
        Task<Models.Game.InventoryItem> EnchantItemAsync(Guid characterId, Guid inventoryItemId);

        /// <summary>Get player's shared stash.</summary>
        Task<List<Models.Game.Stash>> GetStashAsync(Guid playerId);

        /// <summary>Move item from character inventory to stash.</summary>
        Task MoveToStashAsync(Guid characterId, Guid playerId, Guid inventoryItemId, int quantity);

        /// <summary>Move item from stash to character inventory.</summary>
        Task MoveFromStashAsync(Guid playerId, Guid characterId, Guid stashId, int quantity);
    }

    public class ConsumableUseResult
    {
        public int HpRestored { get; set; }
        public int MpRestored { get; set; }
        public string? BuffApplied { get; set; }
        public string Message { get; set; } = null!;
    }
}
