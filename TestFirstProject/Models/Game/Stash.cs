namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Shared storage item across all of a player's characters.
    /// </summary>
    public class Stash
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public AppUser Player { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; } = 1;
    }
}
