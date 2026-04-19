using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Rolls loot from enemy loot tables and awards items to a character's inventory.
    /// </summary>
    public class LootService : ILootService
    {
        private readonly PersonsContext _db;

        public LootService(PersonsContext db)
        {
            _db = db;
        }

        public async Task<List<LootDropResponse>> RollLootAsync(Guid enemyId, Guid characterId)
        {
            var lootEntries = await _db.LootTableEntries
                .Include(e => e.Item)
                .Where(e => e.EnemyId == enemyId)
                .ToListAsync();

            if (lootEntries.Count == 0)
                return new List<LootDropResponse>();

            // Determine which items drop first, then batch-load existing inventory
            var droppedItems = new List<(LootTableEntry Entry, int Quantity)>();
            foreach (var entry in lootEntries)
            {
                if (Random.Shared.NextDouble() * 100 > entry.DropChance)
                    continue;

                var quantity = Random.Shared.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                droppedItems.Add((entry, quantity));
            }

            if (droppedItems.Count == 0)
                return new List<LootDropResponse>();

            // Batch-load existing inventory items to avoid N+1
            var droppedItemIds = droppedItems.Select(d => d.Entry.ItemId).Distinct().ToList();
            var existingItems = await _db.InventoryItems
                .Where(i => i.CharacterId == characterId && droppedItemIds.Contains(i.ItemId))
                .ToDictionaryAsync(i => i.ItemId);

            var drops = new List<LootDropResponse>();
            foreach (var (entry, quantity) in droppedItems)
            {
                InventoryHelper.AddOrIncrement(_db, characterId, entry.ItemId, quantity, existingItems);

                drops.Add(new LootDropResponse(
                    entry.Item.Name,
                    entry.Item.Rarity.ToString(),
                    quantity
                ));
            }

            // Caller (CombatEngine) is responsible for SaveChangesAsync
            return drops;
        }
    }
}
