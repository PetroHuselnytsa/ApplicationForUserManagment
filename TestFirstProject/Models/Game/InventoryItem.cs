namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// An item in a character's inventory with quantity and enchantment tracking.
    /// </summary>
    public class InventoryItem
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        // Enchantment bonus (added on top of item's base bonus)
        public int EnchantmentLevel { get; set; } = 0;
        public int EnchantBonusAttack { get; set; }
        public int EnchantBonusDefense { get; set; }
        public int EnchantBonusHp { get; set; }
        public int EnchantBonusMagicPower { get; set; }
    }
}
