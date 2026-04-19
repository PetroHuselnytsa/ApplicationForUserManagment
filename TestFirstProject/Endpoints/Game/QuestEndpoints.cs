using TestFirstProject.Extensions;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Maps quest endpoints: list active/available quests, accept, complete.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class QuestEndpoints
    {
        public static void MapQuestEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/characters/{characterId}/quests")
                           .WithTags("Quests")
                           .RequireAuthorization();

            // GET /characters/{characterId}/quests — active and available quests
            group.MapGet("/", async (Guid characterId, IQuestProgressTracker tracker, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var active = await tracker.GetActiveQuestsAsync(playerId, characterId);
                var available = await tracker.GetAvailableQuestsAsync(playerId, characterId);
                return Results.Ok(new { active, available });
            })
            .WithName("GetQuests");

            // POST /characters/{characterId}/quests/{questId}/accept
            group.MapPost("/{questId}/accept", async (Guid characterId, Guid questId, IQuestProgressTracker tracker, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await tracker.AcceptQuestAsync(playerId, characterId, questId);
                return Results.Ok(result);
            })
            .WithName("AcceptQuest");

            // POST /characters/{characterId}/quests/{questId}/complete
            group.MapPost("/{questId}/complete", async (Guid characterId, Guid questId, IQuestProgressTracker tracker, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await tracker.CompleteQuestAsync(playerId, characterId, questId);
                return Results.Ok(result);
            })
            .WithName("CompleteQuest");
        }
    }
}
