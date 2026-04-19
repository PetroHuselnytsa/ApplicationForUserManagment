namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Shared storage across a player's characters.
    /// </summary>
    public class Stash
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; } = 1;

        // Navigation
        public AppUser Player { get; set; } = null!;
        public Item Item { get; set; } = null!;
    }
}
