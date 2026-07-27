using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Immutable record of a completed snake game, used for leaderboard and ELO calculation.
    /// </summary>
    public class GameScore
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int Score { get; set; }
        public GameDifficulty Difficulty { get; set; }
        public int DurationSeconds { get; set; }
        public int EnemiesDefeated { get; set; }
        public int FoodEaten { get; set; }
        public int MaxCombo { get; set; }
        public int RatingChange { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}
