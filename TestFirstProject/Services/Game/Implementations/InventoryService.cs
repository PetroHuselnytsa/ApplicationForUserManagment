using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Inventory management: equip/unequip, use consumables, enchant items, shared stash.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly PersonsContext _db;
        private static readonly string[] ValidSlots = { "Weapon", "Armor", "Accessory" };
        private static readonly string[] EnchantableStats = { "HP", "MP", "Attack", "Defense", "MagicPower", "Speed" };

        public InventoryService(PersonsContext db)
        {
            _db = db;
        }

        public async Task<List<InventoryItemResponse>> GetInventoryAsync(Guid playerId, Guid characterId)
        {
            await VerifyCharacterOwnership(playerId, characterId);

            var items = await _db.InventoryItems
                .Include(i => i.Item)
                .Where(i => i.CharacterId == characterId)
                .OrderBy(i => i.Item.Type)
                .ThenBy(i => i.Item.Name)
                .ToListAsync();

            return items.Select(i => new InventoryItemResponse(
                i.Id, i.ItemId, i.Item.Name, i.Item.Description,
                i.Item.Type.ToString(), i.Item.Rarity.ToString(),
                i.Quantity, i.EnchantmentStat, i.EnchantmentValue
            )).ToList();
        }

        public async Task<EquippedItemResponse> EquipItemAsync(Guid playerId, Guid characterId, EquipItemRequest request)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            if (!ValidSlots.Contains(request.Slot))
                throw new ValidationException($"Invalid slot: {request.Slot}. Valid: Weapon, Armor, Accessory");

            var invItem = await _db.InventoryItems
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId && i.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            // Validate item can be equipped in this slot
            var expectedType = request.Slot switch
            {
                "Weapon" => ItemType.Weapon,
                "Armor" => ItemType.Armor,
                "Accessory" => ItemType.Accessory,
                _ => throw new ValidationException("Invalid slot.")
            };

            if (invItem.Item.Type != expectedType)
                throw new ValidationException($"Cannot equip {invItem.Item.Type} in {request.Slot} slot.");

            // Check level requirement
            if (invItem.Item.LevelRequirement > character.Level)
                throw new ValidationException($"Character must be level {invItem.Item.LevelRequirement} to equip this item.");

            // Check class restriction
            if (invItem.Item.ClassRestriction.HasValue && invItem.Item.ClassRestriction.Value != character.Class)
                throw new ValidationException($"This item is restricted to {invItem.Item.ClassRestriction.Value} class.");

            // Remove currently equipped item in this slot (if any)
            var currentEquip = await _db.EquippedItems
                .FirstOrDefaultAsync(e => e.CharacterId == characterId && e.Slot == request.Slot);
            if (currentEquip != null)
                _db.EquippedItems.Remove(currentEquip);

            // Equip the new item
            var equipped = new EquippedItem
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                InventoryItemId = invItem.Id,
                Slot = request.Slot
            };
            _db.EquippedItems.Add(equipped);

            await _db.SaveChangesAsync();

            return new EquippedItemResponse(request.Slot, invItem.Item.Name, invItem.Item.Rarity.ToString(), invItem.Id);
        }

        public async Task<string> UseItemAsync(Guid playerId, Guid characterId, UseItemRequest request)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            var invItem = await _db.InventoryItems
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId && i.CharacterId == characterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            if (invItem.Item.Type != ItemType.Consumable)
                throw new ValidationException("Only consumable items can be used.");

            if (invItem.Quantity <= 0)
                throw new ValidationException("No items remaining.");

            string result;
            switch (invItem.Item.ConsumableType)
            {
                case ConsumableType.HealthPotion:
                    var hpBefore = character.Stats.HP;
                    character.Stats.HP = Math.Min(character.Stats.MaxHP, character.Stats.HP + invItem.Item.ConsumableValue);
                    var hpRestored = character.Stats.HP - hpBefore;
                    result = $"Restored {hpRestored} HP. (HP: {character.Stats.HP}/{character.Stats.MaxHP})";
                    break;

                case ConsumableType.ManaPotion:
                    var mpBefore = character.Stats.MP;
                    character.Stats.MP = Math.Min(character.Stats.MaxMP, character.Stats.MP + invItem.Item.ConsumableValue);
                    var mpRestored = character.Stats.MP - mpBefore;
                    result = $"Restored {mpRestored} MP. (MP: {character.Stats.MP}/{character.Stats.MaxMP})";
                    break;

                case ConsumableType.Scroll:
                    result = $"Used {invItem.Item.Name}. Effect applies during next combat.";
                    break;

                default:
                    throw new ValidationException("Unknown consumable type.");
            }

            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                _db.InventoryItems.Remove(invItem);

            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<InventoryItemResponse> EnchantItemAsync(Guid playerId, Guid characterId, EnchantItemRequest request)
        {
            await VerifyCharacterOwnership(playerId, characterId);

            var targetItem = await _db.InventoryItems
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId && i.CharacterId == characterId)
                ?? throw new NotFoundException("Target item not found in inventory.");

            // Only equipment can be enchanted
            if (targetItem.Item.Type != ItemType.Weapon && targetItem.Item.Type != ItemType.Armor &&
                targetItem.Item.Type != ItemType.Accessory)
                throw new ValidationException("Only equipment items (Weapon, Armor, Accessory) can be enchanted.");

            // Material item required
            var materialItem = await _db.InventoryItems
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == request.MaterialItemId && i.CharacterId == characterId)
                ?? throw new NotFoundException("Material item not found in inventory.");

            if (materialItem.Item.Type != ItemType.Material)
                throw new ValidationException("The second item must be a Material type.");

            if (materialItem.Quantity <= 0)
                throw new ValidationException("No material items remaining.");

            // Determine enchantment based on material rarity
            var enchantValue = materialItem.Item.Rarity switch
            {
                ItemRarity.Common => 2,
                ItemRarity.Uncommon => 4,
                ItemRarity.Rare => 7,
                ItemRarity.Epic => 12,
                ItemRarity.Legendary => 20,
                _ => 2
            };

            // Pick a random stat to enchant
            var statToEnchant = EnchantableStats[Random.Shared.Next(EnchantableStats.Length)];

            // Apply enchantment (replaces existing enchantment)
            targetItem.EnchantmentStat = statToEnchant;
            targetItem.EnchantmentValue = enchantValue;

            // Consume material
            materialItem.Quantity--;
            if (materialItem.Quantity <= 0)
                _db.InventoryItems.Remove(materialItem);

            await _db.SaveChangesAsync();

            return new InventoryItemResponse(
                targetItem.Id, targetItem.ItemId, targetItem.Item.Name, targetItem.Item.Description,
                targetItem.Item.Type.ToString(), targetItem.Item.Rarity.ToString(),
                targetItem.Quantity, targetItem.EnchantmentStat, targetItem.EnchantmentValue
            );
        }

        public async Task<List<StashItemResponse>> GetStashAsync(Guid playerId)
        {
            var stashItems = await _db.Stashes
                .Include(s => s.Item)
                .Where(s => s.PlayerId == playerId)
                .OrderBy(s => s.Item.Name)
                .ToListAsync();

            return stashItems.Select(s => new StashItemResponse(
                s.Id, s.ItemId, s.Item.Name, s.Item.Type.ToString(),
                s.Item.Rarity.ToString(), s.Quantity
            )).ToList();
        }

        private async Task VerifyCharacterOwnership(Guid playerId, Guid characterId)
        {
            var exists = await _db.Characters
                .AnyAsync(c => c.Id == characterId && c.PlayerId == playerId);
            if (!exists)
                throw new NotFoundException("Character not found.");
        }
    }
}
