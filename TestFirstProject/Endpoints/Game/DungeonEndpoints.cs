using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Dungeon run endpoints: start runs, view state, act in rooms.
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
            group.MapPost("/{zoneId:guid}/start", async (
                Guid zoneId,
                StartDungeonRequest request,
                IDungeonRunner dungeonRunner,
                HttpContext httpContext) =>
            {
                var run = await dungeonRunner.StartDungeonAsync(request.CharacterId, zoneId);
                var dto = MapRunToDto(run);
                return Results.Created($"/api/dungeons/runs/{run.Id}", dto);
            })
            .WithName("StartDungeon");

            // GET /dungeons/runs/{id} — current run state
            group.MapGet("/runs/{id:guid}", async (
                Guid id,
                IDungeonRunner dungeonRunner,
                HttpContext httpContext) =>
            {
                var run = await dungeonRunner.GetDungeonRunAsync(id);
                var dto = MapRunToDto(run);
                return Results.Ok(dto);
            })
            .WithName("GetDungeonRun");

            // POST /dungeons/runs/{id}/action — act in current room
            group.MapPost("/runs/{id:guid}/action", async (
                Guid id,
                DungeonActionRequest request,
                IDungeonRunner dungeonRunner,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                // We need characterId from the dungeon run
                var run = await dungeonRunner.GetDungeonRunAsync(id);
                var result = await dungeonRunner.ActInRoomAsync(id, run.CharacterId, request.Action);

                var dto = new DungeonRoomResultDto(
                    RoomType: result.RoomType,
                    Status: result.Status,
                    Message: result.Message,
                    BattleId: result.BattleId,
                    TrapDamage: result.TrapDamage,
                    HpRestored: result.HpRestored,
                    MpRestored: result.MpRestored,
                    TreasureItemName: result.TreasureItemName,
                    IsDungeonComplete: result.IsDungeonComplete
                );

                return Results.Ok(dto);
            })
            .WithName("DungeonAction");
        }

        private static DungeonRunDto MapRunToDto(Models.Game.DungeonRun run)
        {
            return new DungeonRunDto(
                Id: run.Id,
                ZoneName: run.Zone?.Name ?? "Unknown",
                Status: run.Status.ToString(),
                CurrentRoomIndex: run.CurrentRoomIndex,
                TotalRooms: run.Rooms.Count,
                Rooms: run.Rooms.OrderBy(r => r.RoomIndex).Select(r => new DungeonRoomDto(
                    RoomIndex: r.RoomIndex,
                    Type: r.Type.ToString(),
                    Status: r.Status.ToString(),
                    EnemyName: r.Enemy?.Name,
                    IsBoss: r.Enemy?.IsBoss ?? false
                )).ToList(),
                StartedAt: run.StartedAt,
                CompletedAt: run.CompletedAt
            );
        }
    }
}
