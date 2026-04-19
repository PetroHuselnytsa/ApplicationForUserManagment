using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Item template definition (seeded data). Instances in inventory via InventoryItem.
    /// </summary>
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }

        /// <summary>Minimum character level required to equip.</summary>
        public int LevelRequirement { get; set; } = 1;

        /// <summary>Class restriction; null means any class can use.</summary>
        public CharacterClass? ClassRestriction { get; set; }

        // Stat bonuses (for equipment)
        public int BonusHP { get; set; }
        public int BonusMP { get; set; }
        public int BonusAttack { get; set; }
        public int BonusDefense { get; set; }
        public int BonusMagicPower { get; set; }
        public int BonusSpeed { get; set; }
        public double BonusCritChance { get; set; }
        public double BonusDodgeChance { get; set; }

        // Consumable fields
        public ConsumableType? ConsumableType { get; set; }

        /// <summary>Heal/restore amount for consumables.</summary>
        public int ConsumableValue { get; set; }

        /// <summary>For scroll consumables: status effect applied as a temp buff.</summary>
        public StatusEffectType? ScrollEffect { get; set; }

        /// <summary>Duration in turns for scroll effect.</summary>
        public int ScrollEffectDuration { get; set; }

        /// <summary>Gold value for buying/selling.</summary>
        public int GoldValue { get; set; }

        // Navigation
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<LootTableEntry> LootEntries { get; set; } = new List<LootTableEntry>();
    }
}
