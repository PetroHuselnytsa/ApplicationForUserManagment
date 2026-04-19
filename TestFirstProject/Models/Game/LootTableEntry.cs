namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// An entry in an enemy's loot table defining what can drop and with what probability.
    /// </summary>
    public class LootTableEntry
    {
        public Guid Id { get; set; }
        public Guid EnemyId { get; set; }
        public Enemy Enemy { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        /// <summary>Drop chance as a percentage (0.0 to 100.0).</summary>
        public double DropChance { get; set; }

        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 1;
    }
}
