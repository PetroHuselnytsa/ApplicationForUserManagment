using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Maps combat endpoints: start battle, get state, submit turn action.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class BattleEndpoints
    {
        public static void MapBattleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/battles")
                           .WithTags("Battles")
                           .RequireAuthorization();

            // POST /battles — start a battle
            group.MapPost("/", async (StartBattleRequest request, ICombatEngine engine, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await engine.StartBattleAsync(playerId, request);
                return Results.Created($"/api/battles/{result.BattleId}", result);
            })
            .WithName("StartBattle");

            // GET /battles/{id} — get current battle state
            group.MapGet("/{id}", async (Guid id, ICombatEngine engine, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await engine.GetBattleStateAsync(playerId, id);
                return Results.Ok(result);
            })
            .WithName("GetBattleState");

            // POST /battles/{id}/action — submit a turn action
            group.MapPost("/{id}/action", async (Guid id, BattleActionRequest request, ICombatEngine engine, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await engine.SubmitActionAsync(playerId, id, request);
                return Results.Ok(result);
            })
            .WithName("SubmitBattleAction");
        }
    }
}
