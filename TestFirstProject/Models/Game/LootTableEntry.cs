namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A single loot drop entry for an enemy.
    /// </summary>
    public class LootTableEntry
    {
        public Guid Id { get; set; }
        public Guid EnemyId { get; set; }
        public Guid ItemId { get; set; }

        /// <summary>Drop chance as a percentage (0-100).</summary>
        public double DropChance { get; set; }

        /// <summary>Minimum quantity dropped.</summary>
        public int MinQuantity { get; set; } = 1;

        /// <summary>Maximum quantity dropped.</summary>
        public int MaxQuantity { get; set; } = 1;

        // Navigation
        public Enemy Enemy { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
