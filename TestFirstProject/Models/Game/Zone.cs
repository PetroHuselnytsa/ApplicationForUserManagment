namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A game zone with a level range, available enemies, a boss, and quests.
    /// </summary>
    public class Zone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }

        // Navigation
        public ICollection<Enemy> Enemies { get; set; } = new List<Enemy>();
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();
    }
}
