using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Shared helper for the "add item to inventory or increment quantity" pattern
    /// used by LootService, DungeonRunner, and QuestProgressTracker.
    /// Does NOT call SaveChangesAsync — the caller is responsible for flushing.
    /// </summary>
    public static class InventoryHelper
    {
        public static async Task AddOrIncrementAsync(PersonsContext db, Guid characterId, Guid itemId, int quantity = 1)
        {
            var existing = await db.InventoryItems
                .FirstOrDefaultAsync(i => i.CharacterId == characterId && i.ItemId == itemId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                db.InventoryItems.Add(new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    CharacterId = characterId,
                    ItemId = itemId,
                    Quantity = quantity
                });
            }
        }

        /// <summary>
        /// Batch-aware version: accepts a pre-loaded dictionary to avoid N+1 queries in loops.
        /// </summary>
        public static void AddOrIncrement(PersonsContext db, Guid characterId, Guid itemId, int quantity,
            Dictionary<Guid, InventoryItem> existingItems)
        {
            if (existingItems.TryGetValue(itemId, out var existing))
            {
                existing.Quantity += quantity;
            }
            else
            {
                var newItem = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    CharacterId = characterId,
                    ItemId = itemId,
                    Quantity = quantity
                };
                db.InventoryItems.Add(newItem);
                existingItems[itemId] = newItem;
            }
        }
    }
}
