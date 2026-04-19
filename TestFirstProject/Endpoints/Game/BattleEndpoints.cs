using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Combat endpoints: start battles, view state, submit turn actions.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class BattleEndpoints
    {
        public static void MapBattleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/battles")
                           .WithTags("Battles")
                           .RequireAuthorization();

            // POST /battles — start a new battle
            group.MapPost("/", async (
                StartBattleRequest request,
                ICombatEngine combatEngine,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();

                // Verify character belongs to player (combat engine handles this internally)
                var battle = await combatEngine.StartBattleAsync(request.CharacterId, request.EnemyId);

                var dto = MapBattleToDto(battle);
                return Results.Created($"/api/battles/{battle.Id}", dto);
            })
            .WithName("StartBattle");

            // GET /battles/{id} — get current battle state
            group.MapGet("/{id:guid}", async (
                Guid id,
                ICombatEngine combatEngine,
                HttpContext httpContext) =>
            {
                var battle = await combatEngine.GetBattleStateAsync(id);
                var dto = MapBattleToDto(battle);
                return Results.Ok(dto);
            })
            .WithName("GetBattleState");

            // POST /battles/{id}/action — submit a turn action
            group.MapPost("/{id:guid}/action", async (
                Guid id,
                BattleActionRequest request,
                ICombatEngine combatEngine,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();

                if (!Enum.TryParse<BattleActionType>(request.ActionType, true, out var actionType))
                    return Results.BadRequest(new { message = "Invalid action type. Options: Attack, UseSkill, UseItem, Flee." });

                var result = await combatEngine.SubmitActionAsync(
                    id, playerId, actionType,
                    request.SkillId, request.ItemId, request.TargetId);

                var dto = new BattleTurnResultDto(
                    BattleId: result.BattleId,
                    Status: result.Status.ToString(),
                    TurnNumber: result.TurnNumber,
                    Log: result.Log.Select(l => new BattleLogEntryDto(
                        Actor: l.Actor,
                        Action: l.Action,
                        Target: l.Target,
                        Damage: l.Damage,
                        Healing: l.Healing,
                        IsCritical: l.IsCritical,
                        IsDodged: l.IsDodged,
                        StatusEffect: l.StatusEffect,
                        Message: l.Message
                    )).ToList(),
                    Loot: result.Loot?.Select(l => new DroppedItemDto(l.ItemId, l.ItemName, l.Quantity)).ToList(),
                    XpAwarded: result.XpAwarded,
                    GoldAwarded: result.GoldAwarded
                );

                return Results.Ok(dto);
            })
            .WithName("SubmitBattleAction");
        }

        private static BattleStateDto MapBattleToDto(Models.Game.Battle battle)
        {
            return new BattleStateDto(
                Id: battle.Id,
                Status: battle.Status.ToString(),
                CurrentTurn: battle.CurrentTurn,
                Participants: battle.Participants.Select(p => new BattleParticipantDto(
                    Id: p.Id,
                    Name: p.Name,
                    Type: p.Type.ToString(),
                    CurrentHp: p.CurrentHp,
                    MaxHp: p.MaxHp,
                    CurrentMp: p.CurrentMp,
                    MaxMp: p.MaxMp,
                    Attack: p.Attack,
                    Defense: p.Defense,
                    Speed: p.Speed,
                    IsAlive: p.IsAlive,
                    IsPhaseTwo: p.IsPhaseTwo
                )).ToList(),
                StatusEffects: battle.StatusEffects.Select(e => new ActiveStatusEffectDto(
                    Type: e.Type.ToString(),
                    TargetParticipantId: e.TargetParticipantId,
                    TargetName: battle.Participants.FirstOrDefault(p => p.Id == e.TargetParticipantId)?.Name ?? "Unknown",
                    RemainingTurns: e.RemainingTurns,
                    TickValue: e.TickValue,
                    Stacks: e.Stacks
                )).ToList()
            );
        }
    }
}
