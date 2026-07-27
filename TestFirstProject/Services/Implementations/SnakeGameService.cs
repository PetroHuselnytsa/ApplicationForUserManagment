using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class SnakeGameService : ISnakeGameService
    {
        private readonly PersonsContext _context;

        // ELO constants
        private const int StartingRating = 1000;
        private const int KFactor = 32;
        private const double RatingAnchor = 1000.0;

        // Expected score thresholds per difficulty
        private static readonly Dictionary<GameDifficulty, int> ExpectedScoreThresholds = new()
        {
            { GameDifficulty.Easy, 50 },
            { GameDifficulty.Medium, 150 },
            { GameDifficulty.Hard, 300 }
        };

        // Anti-fraud caps
        private const int MinDurationSeconds = 5;
        private const int MaxScorePerSecond = 50;
        private const int MaxEnemiesPerMinute = 10;

        public SnakeGameService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<SubmitScoreResponse> SubmitScoreAsync(Guid currentUserId, SubmitScoreRequest request)
        {
            // Validate difficulty
            if (!Enum.TryParse<GameDifficulty>(request.Difficulty, true, out var difficulty))
                throw new ValidationException($"Invalid difficulty '{request.Difficulty}'. Must be Easy, Medium, or Hard.");

            // Anti-fraud checks
            if (request.DurationSeconds < MinDurationSeconds)
                throw new ValidationException($"Game duration must be at least {MinDurationSeconds} seconds.");

            if (request.Score < 0)
                throw new ValidationException("Score cannot be negative.");

            var maxAllowedScore = request.DurationSeconds * MaxScorePerSecond;
            if (request.Score > maxAllowedScore)
                throw new ValidationException("Score exceeds maximum allowed for the game duration.");

            var maxAllowedEnemies = (int)Math.Ceiling(request.DurationSeconds / 60.0 * MaxEnemiesPerMinute);
            if (request.EnemiesDefeated > maxAllowedEnemies)
                throw new ValidationException("Enemies defeated exceeds maximum possible for the game duration.");

            // Verify user exists
            var userExists = await _context.AppUsers.AnyAsync(u => u.Id == currentUserId);
            if (!userExists)
                throw new NotFoundException("User not found.");

            // Get or create player rating
            var playerRating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.UserId == currentUserId);

            if (playerRating == null)
            {
                playerRating = new PlayerRating
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    Rating = StartingRating
                };
                _context.PlayerRatings.Add(playerRating);
            }

            // Calculate ELO change
            int ratingChange = CalculateRatingChange(playerRating.Rating, request.Score, difficulty);

            // Update rating
            playerRating.Rating = Math.Max(0, playerRating.Rating + ratingChange);
            playerRating.GamesPlayed++;
            playerRating.LastPlayedAt = DateTime.UtcNow;
            if (request.Score > playerRating.HighestScore)
                playerRating.HighestScore = request.Score;

            // Save game score
            var gameScore = new GameScore
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Score = request.Score,
                Difficulty = difficulty,
                DurationSeconds = request.DurationSeconds,
                EnemiesDefeated = request.EnemiesDefeated,
                FoodEaten = request.FoodEaten,
                MaxCombo = request.MaxCombo,
                RatingChange = ratingChange,
                PlayedAt = DateTime.UtcNow
            };
            _context.GameScores.Add(gameScore);

            await _context.SaveChangesAsync();

            return new SubmitScoreResponse(
                gameScore.Id,
                gameScore.Score,
                ratingChange,
                playerRating.Rating,
                playerRating.GamesPlayed
            );
        }

        public async Task<LeaderboardResponse> GetLeaderboardAsync(string period, int page = 1, int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.GameScores
                .Include(g => g.User)
                .AsQueryable();

            // Apply period filter
            var now = DateTime.UtcNow;
            switch (period.ToLowerInvariant())
            {
                case "daily":
                    var startOfDay = now.Date;
                    query = query.Where(g => g.PlayedAt >= startOfDay);
                    break;
                case "weekly":
                    var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                    query = query.Where(g => g.PlayedAt >= startOfWeek);
                    break;
                case "all":
                    break;
                default:
                    throw new ValidationException($"Invalid period '{period}'. Must be 'all', 'daily', or 'weekly'.");
            }

            var totalCount = await query.CountAsync();

            var entries = await query
                .OrderByDescending(g => g.Score)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new LeaderboardEntryDto(
                    0, // Rank computed below
                    g.UserId,
                    g.User.Username,
                    g.Score,
                    g.Difficulty.ToString(),
                    g.DurationSeconds,
                    g.EnemiesDefeated,
                    g.PlayedAt
                ))
                .ToListAsync();

            // Compute rank based on page offset
            var rankedEntries = entries
                .Select((e, i) => e with { Rank = (page - 1) * pageSize + i + 1 })
                .ToList();

            return new LeaderboardResponse(rankedEntries, totalCount, page, pageSize, period.ToLowerInvariant());
        }

        public async Task<PlayerStatsDto> GetPlayerStatsAsync(Guid currentUserId)
        {
            var rating = await _context.PlayerRatings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.UserId == currentUserId);

            if (rating == null)
            {
                return new PlayerStatsDto(StartingRating, 0, 0, null);
            }

            return new PlayerStatsDto(
                rating.Rating,
                rating.GamesPlayed,
                rating.HighestScore,
                rating.LastPlayedAt
            );
        }

        // --- Private helpers ---

        /// <summary>
        /// ELO calculation adapted for single-player:
        /// - Each difficulty has an expected-score threshold (Easy: 50, Medium: 150, Hard: 300).
        /// - Actual performance = min(1, actualScore / (2 * threshold)).
        /// - Expected performance from standard logistic curve using current rating vs anchor (1000).
        /// - Rating delta = K * (actual - expected).
        /// </summary>
        private int CalculateRatingChange(int currentRating, int actualScore, GameDifficulty difficulty)
        {
            var threshold = ExpectedScoreThresholds[difficulty];

            // Actual performance: capped at 1.0
            double actualPerformance = Math.Min(1.0, (double)actualScore / (2.0 * threshold));

            // Expected performance: logistic function based on current rating vs anchor
            double expectedPerformance = 1.0 / (1.0 + Math.Pow(10.0, (RatingAnchor - currentRating) / 400.0));

            // Rating change
            double delta = KFactor * (actualPerformance - expectedPerformance);

            return (int)Math.Round(delta);
        }
    }
}
