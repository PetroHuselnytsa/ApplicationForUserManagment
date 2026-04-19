namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Tracks which inventory item is equipped in which slot on a character.
    /// </summary>
    public class EquippedItem
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid InventoryItemId { get; set; }

        /// <summary>Equipment slot: "Weapon", "Armor", "Accessory".</summary>
        public string Slot { get; set; } = null!;

        // Navigation
        public Character Character { get; set; } = null!;
        public InventoryItem InventoryItem { get; set; } = null!;
    }
}
