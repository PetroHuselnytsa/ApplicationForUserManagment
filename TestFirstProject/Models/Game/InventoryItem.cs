namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// An item instance in a character's inventory.
    /// </summary>
    public class InventoryItem
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; } = 1;

        /// <summary>Enchantment bonus stat name, if enchanted.</summary>
        public string? EnchantmentStat { get; set; }

        /// <summary>Enchantment bonus value.</summary>
        public int EnchantmentValue { get; set; }

        // Navigation
        public Character Character { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
