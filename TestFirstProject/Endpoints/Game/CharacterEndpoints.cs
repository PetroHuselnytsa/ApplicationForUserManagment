using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Maps character management endpoints: create, list, stats, allocate skill points.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class CharacterEndpoints
    {
        public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/characters")
                           .WithTags("Characters")
                           .RequireAuthorization();

            // POST /characters — create character
            group.MapPost("/", async (CreateCharacterRequest request, ICharacterProgressionService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.CreateCharacterAsync(playerId, request);
                return Results.Created($"/api/characters/{result.Id}", result);
            })
            .WithName("CreateCharacter");

            // GET /characters — list player's characters
            group.MapGet("/", async (ICharacterProgressionService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.GetCharactersAsync(playerId);
                return Results.Ok(result);
            })
            .WithName("ListCharacters");

            // GET /characters/{id}/stats — full stat sheet with equipment bonuses
            group.MapGet("/{id}/stats", async (Guid id, ICharacterProgressionService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.GetCharacterStatsAsync(playerId, id);
                return Results.Ok(result);
            })
            .WithName("GetCharacterStats");

            // POST /characters/{id}/allocate-stats — spend skill points
            group.MapPost("/{id}/allocate-stats", async (Guid id, AllocateStatsRequest request, ICharacterProgressionService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.AllocateStatsAsync(playerId, id, request);
                return Results.Ok(result);
            })
            .WithName("AllocateStats");
        }
    }
}
