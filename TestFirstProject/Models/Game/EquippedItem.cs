using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Represents an item currently equipped by a character in a specific slot.
    /// </summary>
    public class EquippedItem
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;

        /// <summary>Equipment slot: Weapon, Armor, or Accessory.</summary>
        public ItemType Slot { get; set; }
    }
}
