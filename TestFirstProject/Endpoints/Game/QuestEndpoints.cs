using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Quest management endpoints: view, accept, and complete quests.
    /// Also includes zone listing for exploration.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class QuestEndpoints
    {
        public static void MapQuestEndpoints(this IEndpointRouteBuilder app)
        {
            // --- Quest endpoints ---
            var questGroup = app.MapGroup("/api/characters/{characterId:guid}/quests")
                                .WithTags("Quests")
                                .RequireAuthorization();

            // GET /characters/{id}/quests — active and available quests
            questGroup.MapGet("/", async (
                Guid characterId,
                IQuestProgressTracker questTracker,
                HttpContext httpContext) =>
            {
                var result = await questTracker.GetQuestsAsync(characterId);

                var dto = new CharacterQuestsDto(
                    ActiveQuests: result.ActiveQuests.Select(aq => new ActiveQuestDto(
                        QuestId: aq.QuestId,
                        Name: aq.Quest.Name,
                        Description: aq.Quest.Description,
                        Type: aq.Quest.Type.ToString(),
                        CurrentCount: aq.CurrentCount,
                        RequiredCount: aq.Quest.RequiredCount,
                        AcceptedAt: aq.AcceptedAt,
                        Deadline: aq.Deadline
                    )).ToList(),
                    AvailableQuests: result.AvailableQuests.Select(q => new AvailableQuestDto(
                        Id: q.Id,
                        Name: q.Name,
                        Description: q.Description,
                        Type: q.Type.ToString(),
                        RequiredCount: q.RequiredCount,
                        MinLevelRequirement: q.MinLevelRequirement,
                        XpReward: q.XpReward,
                        GoldReward: q.GoldReward,
                        RewardItemName: q.RewardItem?.Name
                    )).ToList(),
                    CompletedQuests: result.CompletedQuests.Select(cq => new CompletedQuestDto(
                        QuestId: cq.QuestId,
                        Name: cq.Quest.Name,
                        CompletedAt: cq.CompletedAt
                    )).ToList()
                );

                return Results.Ok(dto);
            })
            .WithName("GetCharacterQuests");

            // POST /characters/{id}/quests/{questId}/accept — accept a quest
            questGroup.MapPost("/{questId:guid}/accept", async (
                Guid characterId,
                Guid questId,
                IQuestProgressTracker questTracker,
                HttpContext httpContext) =>
            {
                var cq = await questTracker.AcceptQuestAsync(characterId, questId);
                return Results.Ok(new { message = "Quest accepted.", questId = cq.QuestId, status = cq.Status.ToString() });
            })
            .WithName("AcceptQuest");

            // POST /characters/{id}/quests/{questId}/complete — try to complete a quest
            questGroup.MapPost("/{questId:guid}/complete", async (
                Guid characterId,
                Guid questId,
                IQuestProgressTracker questTracker,
                HttpContext httpContext) =>
            {
                var result = await questTracker.CompleteQuestAsync(characterId, questId);

                var dto = new QuestCompletionResultDto(
                    Success: result.Success,
                    Message: result.Message,
                    XpReward: result.XpReward,
                    GoldReward: result.GoldReward,
                    RewardItemName: result.RewardItemName,
                    NextQuestId: result.NextQuestId
                );

                return result.Success ? Results.Ok(dto) : Results.BadRequest(dto);
            })
            .WithName("CompleteQuest");

            // --- Zone exploration endpoints ---
            var zoneGroup = app.MapGroup("/api/zones")
                               .WithTags("Zones")
                               .RequireAuthorization();

            // GET /zones — list all zones with enemies
            zoneGroup.MapGet("/", async (
                GameDbContext db,
                HttpContext httpContext) =>
            {
                var zones = await db.Zones
                    .Include(z => z.Enemies)
                    .OrderBy(z => z.MinLevel)
                    .ToListAsync();

                var dtos = zones.Select(z => new ZoneDto(
                    Id: z.Id,
                    Name: z.Name,
                    Description: z.Description,
                    MinLevel: z.MinLevel,
                    MaxLevel: z.MaxLevel,
                    Enemies: z.Enemies.Select(e => new ZoneEnemyDto(
                        Id: e.Id,
                        Name: e.Name,
                        IsBoss: e.IsBoss,
                        DamageType: e.PrimaryDamageType.ToString()
                    )).ToList()
                )).ToList();

                return Results.Ok(dtos);
            })
            .WithName("GetZones");
        }
    }
}
