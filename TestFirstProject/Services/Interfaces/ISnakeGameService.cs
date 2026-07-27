using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles snake game business logic: score submission, ELO rating, leaderboards.
    /// </summary>
    public interface ISnakeGameService
    {
        /// <summary>Submit a completed game score and update ELO rating.</summary>
        Task<SubmitScoreResponse> SubmitScoreAsync(Guid currentUserId, SubmitScoreRequest request);

        /// <summary>Get leaderboard entries for a given period (all, daily, weekly).</summary>
        Task<LeaderboardResponse> GetLeaderboardAsync(string period, int page = 1, int pageSize = 20);

        /// <summary>Get the current player's stats and rating.</summary>
        Task<PlayerStatsDto> GetPlayerStatsAsync(Guid currentUserId);
    }
}
