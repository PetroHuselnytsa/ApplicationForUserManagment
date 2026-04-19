using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Inventory management: equip/unequip, use consumables, enchant, stash.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>List all items in a character's inventory.</summary>
        Task<List<InventoryItemResponse>> GetInventoryAsync(Guid playerId, Guid characterId);

        /// <summary>Equip an item to a slot, replacing any currently equipped item.</summary>
        Task<EquippedItemResponse> EquipItemAsync(Guid playerId, Guid characterId, EquipItemRequest request);

        /// <summary>Use a consumable item (health/mana potion or scroll).</summary>
        Task<string> UseItemAsync(Guid playerId, Guid characterId, UseItemRequest request);

        /// <summary>Enchant an equipment item using a material item.</summary>
        Task<InventoryItemResponse> EnchantItemAsync(Guid playerId, Guid characterId, EnchantItemRequest request);

        /// <summary>Get the player's shared stash.</summary>
        Task<List<StashItemResponse>> GetStashAsync(Guid playerId);
    }
}
