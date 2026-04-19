using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Handles loot rolling, inventory management, equipment, enchanting, and stash.
    /// </summary>
    public class LootService : ILootService
    {
        private readonly GameDbContext _db;
        private static readonly Random _rng = new();

        // Enchantment stone item ID (from seed data)
        private static readonly Guid EnchantmentStoneId = new("c0000003-0000-0000-0000-000000000001");

        public LootService(GameDbContext db)
        {
            _db = db;
        }

        public async Task<List<DroppedItem>> RollLootAsync(Guid enemyId)
        {
            var lootTable = await _db.LootTableEntries
                .Include(l => l.Item)
                .Where(l => l.EnemyId == enemyId)
                .ToListAsync();

            var drops = new List<DroppedItem>();

            foreach (var entry in lootTable)
            {
                if (_rng.NextDouble() * 100.0 < entry.DropChance)
                {
                    var quantity = _rng.Next(entry.MinQuantity, entry.MaxQuantity + 1);
                    drops.Add(new DroppedItem
                    {
                        ItemId = entry.ItemId,
                        ItemName = entry.Item.Name,
                        Quantity = quantity
                    });
                }
            }

            return drops;
        }

        public async Task AddItemsToInventoryAsync(Guid characterId, List<DroppedItem> items)
        {
            foreach (var drop in items)
            {
                // Check if character already has this item in inventory
                var existing = await _db.InventoryItems
                    .FirstOrDefaultAsync(ii => ii.CharacterId == characterId && ii.ItemId == drop.ItemId);

                if (existing != null)
                {
                    existing.Quantity += drop.Quantity;
                }
                else
                {
                    _db.InventoryItems.Add(new InventoryItem
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = characterId,
                        ItemId = drop.ItemId,
                        Quantity = drop.Quantity
                    });
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<InventoryItem>> GetInventoryAsync(Guid characterId)
        {
            return await _db.InventoryItems
                .Include(ii => ii.Item)
                .Where(ii => ii.CharacterId == characterId)
                .OrderBy(ii => ii.Item.Type)
                .ThenBy(ii => ii.Item.Name)
                .ToListAsync();
        }

        public async Task EquipItemAsync(Guid characterId, Guid inventoryItemId)
        {
            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            var item = invItem.Item;

            // Validate item can be equipped
            if (item.Type != ItemType.Weapon && item.Type != ItemType.Armor && item.Type != ItemType.Accessory)
                throw new ValidationException("This item cannot be equipped.");

            // Check level requirement
            var character = await _db.Characters.FindAsync(characterId)
                ?? throw new NotFoundException("Character not found.");

            if (character.Level < item.LevelRequirement)
                throw new ValidationException($"Character must be level {item.LevelRequirement} to equip this item.");

            // Check class restriction
            if (item.ClassRestriction.HasValue && item.ClassRestriction.Value != character.Class)
                throw new ValidationException($"This item can only be equipped by a {item.ClassRestriction.Value}.");

            // Unequip existing item in that slot
            var existingEquip = await _db.EquippedItems
                .FirstOrDefaultAsync(ei => ei.CharacterId == characterId && ei.Slot == item.Type);

            if (existingEquip != null)
            {
                _db.EquippedItems.Remove(existingEquip);
            }

            // Equip the new item
            _db.EquippedItems.Add(new EquippedItem
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                InventoryItemId = inventoryItemId,
                Slot = item.Type
            });

            await _db.SaveChangesAsync();
        }

        public async Task UnequipItemAsync(Guid characterId, ItemType slot)
        {
            var equipped = await _db.EquippedItems
                .FirstOrDefaultAsync(ei => ei.CharacterId == characterId && ei.Slot == slot)
                ?? throw new NotFoundException("No item equipped in that slot.");

            _db.EquippedItems.Remove(equipped);
            await _db.SaveChangesAsync();
        }

        public async Task<ConsumableUseResult> UseConsumableAsync(Guid characterId, Guid inventoryItemId, Guid? battleId = null)
        {
            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            if (invItem.Item.Type != ItemType.Consumable)
                throw new ValidationException("This item is not a consumable.");

            if (invItem.Quantity <= 0)
                throw new ValidationException("No items remaining.");

            var character = await _db.Characters.FindAsync(characterId)
                ?? throw new NotFoundException("Character not found.");

            var item = invItem.Item;
            var result = new ConsumableUseResult();

            // Apply healing
            if (item.HealAmount > 0)
            {
                var totalStats = await _db.CharacterStats.FirstAsync(s => s.CharacterId == characterId);
                var maxHp = totalStats.TotalHp;
                var healed = Math.Min(item.HealAmount, maxHp - character.CurrentHp);
                character.CurrentHp += healed;
                result.HpRestored = healed;
            }

            // Apply mana
            if (item.ManaAmount > 0)
            {
                var totalStats = await _db.CharacterStats.FirstAsync(s => s.CharacterId == characterId);
                var maxMp = totalStats.TotalMp;
                var restored = Math.Min(item.ManaAmount, maxMp - character.CurrentMp);
                character.CurrentMp += restored;
                result.MpRestored = restored;
            }

            // Apply buff
            if (item.BuffEffect.HasValue)
            {
                result.BuffApplied = item.BuffEffect.Value.ToString();
            }

            // Consume the item
            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                _db.InventoryItems.Remove(invItem);

            result.Message = $"Used {item.Name}.";
            if (result.HpRestored > 0) result.Message += $" Restored {result.HpRestored} HP.";
            if (result.MpRestored > 0) result.Message += $" Restored {result.MpRestored} MP.";
            if (result.BuffApplied != null) result.Message += $" Applied {result.BuffApplied}.";

            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<InventoryItem> EnchantItemAsync(Guid characterId, Guid inventoryItemId)
        {
            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            var item = invItem.Item;
            if (item.Type != ItemType.Weapon && item.Type != ItemType.Armor && item.Type != ItemType.Accessory)
                throw new ValidationException("Only equipment can be enchanted.");

            if (invItem.EnchantmentLevel >= 10)
                throw new ValidationException("Item has reached maximum enchantment level.");

            // Check for enchantment materials
            var enchantStone = await _db.InventoryItems
                .FirstOrDefaultAsync(ii => ii.CharacterId == characterId && ii.ItemId == EnchantmentStoneId);

            if (enchantStone == null || enchantStone.Quantity <= 0)
                throw new ValidationException("You need an Enchantment Stone to enchant items.");

            // Consume material
            enchantStone.Quantity--;
            if (enchantStone.Quantity <= 0)
                _db.InventoryItems.Remove(enchantStone);

            // Apply enchantment bonus (+2 per level to primary stats based on item type)
            invItem.EnchantmentLevel++;
            var bonusPerLevel = 2;

            if (item.Type == ItemType.Weapon)
            {
                if (item.BonusAttack > 0) invItem.EnchantBonusAttack += bonusPerLevel;
                if (item.BonusMagicPower > 0) invItem.EnchantBonusMagicPower += bonusPerLevel;
                // Default to attack if no specific bonus
                if (item.BonusAttack == 0 && item.BonusMagicPower == 0)
                    invItem.EnchantBonusAttack += bonusPerLevel;
            }
            else if (item.Type == ItemType.Armor)
            {
                invItem.EnchantBonusDefense += bonusPerLevel;
                invItem.EnchantBonusHp += bonusPerLevel * 3;
            }
            else // Accessory
            {
                invItem.EnchantBonusAttack += 1;
                invItem.EnchantBonusMagicPower += 1;
            }

            await _db.SaveChangesAsync();
            return invItem;
        }

        public async Task<List<Stash>> GetStashAsync(Guid playerId)
        {
            return await _db.Stashes
                .Include(s => s.Item)
                .Where(s => s.PlayerId == playerId)
                .OrderBy(s => s.Item.Name)
                .ToListAsync();
        }

        public async Task MoveToStashAsync(Guid characterId, Guid playerId, Guid inventoryItemId, int quantity)
        {
            if (quantity <= 0)
                throw new ValidationException("Quantity must be positive.");

            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            // Verify character belongs to player
            var character = await _db.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new ForbiddenException("Character does not belong to you.");

            if (invItem.Quantity < quantity)
                throw new ValidationException("Not enough items in inventory.");

            // Check if item is equipped
            var isEquipped = await _db.EquippedItems
                .AnyAsync(ei => ei.InventoryItemId == inventoryItemId);
            if (isEquipped)
                throw new ValidationException("Cannot stash equipped items. Unequip first.");

            // Remove from inventory
            invItem.Quantity -= quantity;
            if (invItem.Quantity <= 0)
                _db.InventoryItems.Remove(invItem);

            // Add to stash
            var stashItem = await _db.Stashes
                .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.ItemId == invItem.ItemId);

            if (stashItem != null)
            {
                stashItem.Quantity += quantity;
            }
            else
            {
                _db.Stashes.Add(new Stash
                {
                    Id = Guid.NewGuid(),
                    PlayerId = playerId,
                    ItemId = invItem.ItemId,
                    Quantity = quantity
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task MoveFromStashAsync(Guid playerId, Guid characterId, Guid stashId, int quantity)
        {
            if (quantity <= 0)
                throw new ValidationException("Quantity must be positive.");

            var stashItem = await _db.Stashes
                .Include(s => s.Item)
                .FirstOrDefaultAsync(s => s.Id == stashId && s.PlayerId == playerId)
                ?? throw new NotFoundException("Stash item not found.");

            // Verify character belongs to player
            var character = await _db.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new ForbiddenException("Character does not belong to you.");

            if (stashItem.Quantity < quantity)
                throw new ValidationException("Not enough items in stash.");

            // Remove from stash
            stashItem.Quantity -= quantity;
            if (stashItem.Quantity <= 0)
                _db.Stashes.Remove(stashItem);

            // Add to inventory
            var invItem = await _db.InventoryItems
                .FirstOrDefaultAsync(ii => ii.CharacterId == characterId && ii.ItemId == stashItem.ItemId);

            if (invItem != null)
            {
                invItem.Quantity += quantity;
            }
            else
            {
                _db.InventoryItems.Add(new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    CharacterId = characterId,
                    ItemId = stashItem.ItemId,
                    Quantity = quantity
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}
