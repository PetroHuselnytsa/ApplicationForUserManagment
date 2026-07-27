namespace TestFirstProject.Models
{
    /// <summary>
    /// Tracks the current ELO-style skill rating for a snake game player.
    /// One row per user; created on first game submission.
    /// </summary>
    public class PlayerRating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int Rating { get; set; } = 1000;
        public int GamesPlayed { get; set; } = 0;
        public int HighestScore { get; set; } = 0;
        public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
    }
}
