using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Definition of an item in the game. Seed data for equipment, consumables, and materials.
    /// </summary>
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }

        // Equipment properties
        public int LevelRequirement { get; set; } = 1;
        public CharacterClass? ClassRestriction { get; set; }
        public int BonusHp { get; set; }
        public int BonusMp { get; set; }
        public int BonusAttack { get; set; }
        public int BonusDefense { get; set; }
        public int BonusMagicPower { get; set; }
        public int BonusSpeed { get; set; }
        public double BonusCritChance { get; set; }
        public double BonusDodgeChance { get; set; }

        // Consumable properties
        public ConsumableType? ConsumableType { get; set; }
        public int HealAmount { get; set; }
        public int ManaAmount { get; set; }
        public StatusEffectType? BuffEffect { get; set; }
        public int BuffDuration { get; set; }

        // Buy/sell values
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
    }
}
