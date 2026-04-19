using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Maps dungeon endpoints: start run, get state, act in room.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class DungeonEndpoints
    {
        public static void MapDungeonEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/dungeons")
                           .WithTags("Dungeons")
                           .RequireAuthorization();

            // POST /dungeons/{zoneId}/start — begin a dungeon run
            group.MapPost("/{zoneId}/start", async (Guid zoneId, StartDungeonRequest request, IDungeonRunner runner, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await runner.StartRunAsync(playerId, request.CharacterId, zoneId);
                return Results.Created($"/api/dungeons/runs/{result.Id}", result);
            })
            .WithName("StartDungeonRun");

            // GET /dungeons/runs/{id} — current run state
            group.MapGet("/runs/{id}", async (Guid id, IDungeonRunner runner, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await runner.GetRunStateAsync(playerId, id);
                return Results.Ok(result);
            })
            .WithName("GetDungeonRunState");

            // POST /dungeons/runs/{id}/action — act in current room
            group.MapPost("/runs/{id}/action", async (Guid id, DungeonActionRequest request, IDungeonRunner runner, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await runner.ActInRoomAsync(playerId, id, request);
                return Results.Ok(result);
            })
            .WithName("ActInDungeonRoom");
        }
    }
}
