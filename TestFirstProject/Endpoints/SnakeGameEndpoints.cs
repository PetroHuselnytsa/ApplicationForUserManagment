using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps snake game endpoints: score submission, leaderboard, player stats.
    /// All endpoints require authentication.
    /// </summary>
    public static class SnakeGameEndpoints
    {
        public static void MapSnakeGameEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/snake")
                           .WithTags("SnakeGame")
                           .RequireAuthorization();

            // POST /api/snake/scores - submit a game score
            group.MapPost("/scores", async (
                SubmitScoreRequest request,
                ISnakeGameService snakeGameService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await snakeGameService.SubmitScoreAsync(userId, request);
                return Results.Created($"/api/snake/scores/{result.GameId}", result);
            })
            .WithName("SubmitSnakeScore");

            // GET /api/snake/leaderboard?period=all&page=1&pageSize=20
            group.MapGet("/leaderboard", async (
                ISnakeGameService snakeGameService,
                string period = "all",
                int page = 1,
                int pageSize = 20) =>
            {
                var result = await snakeGameService.GetLeaderboardAsync(period, page, pageSize);
                return Results.Ok(result);
            })
            .WithName("GetSnakeLeaderboard");

            // GET /api/snake/stats - current user's rating and stats
            group.MapGet("/stats", async (
                ISnakeGameService snakeGameService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await snakeGameService.GetPlayerStatsAsync(userId);
                return Results.Ok(result);
            })
            .WithName("GetSnakePlayerStats");
        }
    }
}
