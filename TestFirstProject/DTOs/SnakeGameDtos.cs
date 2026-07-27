namespace TestFirstProject.DTOs
{
    // --- Snake Game ---

    /// <summary>Request to submit a completed game's score.</summary>
    public record SubmitScoreRequest(
        int Score,
        string Difficulty,
        int DurationSeconds,
        int EnemiesDefeated,
        int FoodEaten,
        int MaxCombo
    );

    /// <summary>Response after score submission with updated rating info.</summary>
    public record SubmitScoreResponse(
        Guid GameId,
        int Score,
        int RatingChange,
        int NewRating,
        int GamesPlayed
    );

    /// <summary>A single entry in the leaderboard.</summary>
    public record LeaderboardEntryDto(
        int Rank,
        Guid UserId,
        string Username,
        int Score,
        string Difficulty,
        int DurationSeconds,
        int EnemiesDefeated,
        DateTime PlayedAt
    );

    /// <summary>Current player's rating and stats.</summary>
    public record PlayerStatsDto(
        int Rating,
        int GamesPlayed,
        int HighestScore,
        DateTime? LastPlayedAt
    );

    /// <summary>Leaderboard response with pagination.</summary>
    public record LeaderboardResponse(
        IReadOnlyList<LeaderboardEntryDto> Entries,
        int TotalCount,
        int Page,
        int PageSize,
        string Period
    );
}
