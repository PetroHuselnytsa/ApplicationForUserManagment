namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A game zone with a level range, enemies, a boss, and available quests.
    /// </summary>
    public class Zone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        /// <summary>Minimum recommended character level.</summary>
        public int MinLevel { get; set; }

        /// <summary>Maximum recommended character level.</summary>
        public int MaxLevel { get; set; }

        // Navigation
        public ICollection<Enemy> Enemies { get; set; } = new List<Enemy>();
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();
        public ICollection<DungeonRun> DungeonRuns { get; set; } = new List<DungeonRun>();
    }
}
