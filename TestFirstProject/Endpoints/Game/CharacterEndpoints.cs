using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Character management endpoints: create, list, view stats, allocate skill points.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class CharacterEndpoints
    {
        public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/characters")
                           .WithTags("Characters")
                           .RequireAuthorization();

            // POST /characters — create a new character
            group.MapPost("/", async (
                CreateCharacterRequest request,
                ICharacterProgressionService progression,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();

                if (!Enum.TryParse<CharacterClass>(request.CharacterClass, true, out var charClass))
                    return Results.BadRequest(new { message = "Invalid character class. Options: Warrior, Mage, Rogue, Paladin." });

                var character = await progression.CreateCharacterAsync(playerId, request.Name, charClass);

                var dto = MapToDto(character, progression);
                return Results.Created($"/api/characters/{character.Id}", dto);
            })
            .WithName("CreateCharacter");

            // GET /characters — list all player's characters
            group.MapGet("/", async (
                ICharacterProgressionService progression,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                var characters = await progression.GetPlayerCharactersAsync(playerId);

                var dtos = characters.Select(c => MapToDto(c, progression)).ToList();
                return Results.Ok(dtos);
            })
            .WithName("GetCharacters");

            // GET /characters/{id}/stats — full stat sheet with equipment bonuses
            group.MapGet("/{id:guid}/stats", async (
                Guid id,
                ICharacterProgressionService progression,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                var character = await progression.GetCharacterAsync(id, playerId);
                var totalStats = await progression.GetTotalStatsAsync(id);

                var dto = new CharacterStatsDto(
                    BaseHp: character.Stats.BaseHp,
                    BaseMp: character.Stats.BaseMp,
                    BaseAttack: character.Stats.BaseAttack,
                    BaseDefense: character.Stats.BaseDefense,
                    BaseMagicPower: character.Stats.BaseMagicPower,
                    BaseSpeed: character.Stats.BaseSpeed,
                    BaseCritChance: character.Stats.BaseCritChance,
                    BaseDodgeChance: character.Stats.BaseDodgeChance,
                    BonusHp: character.Stats.BonusHp,
                    BonusMp: character.Stats.BonusMp,
                    BonusAttack: character.Stats.BonusAttack,
                    BonusDefense: character.Stats.BonusDefense,
                    BonusMagicPower: character.Stats.BonusMagicPower,
                    BonusSpeed: character.Stats.BonusSpeed,
                    TotalHp: totalStats.BaseHp,
                    TotalMp: totalStats.BaseMp,
                    TotalAttack: totalStats.BaseAttack,
                    TotalDefense: totalStats.BaseDefense,
                    TotalMagicPower: totalStats.BaseMagicPower,
                    TotalSpeed: totalStats.BaseSpeed,
                    EquippedItems: character.EquippedItems.Select(ei => new EquippedItemDto(
                        Slot: ei.Slot.ToString(),
                        ItemName: ei.InventoryItem.Item.Name,
                        Rarity: ei.InventoryItem.Item.Rarity.ToString(),
                        EnchantmentLevel: ei.InventoryItem.EnchantmentLevel
                    )).ToList(),
                    LearnedSkills: character.LearnedSkills.Select(ls => new LearnedSkillDto(
                        SkillId: ls.SkillId,
                        Name: ls.Skill.Name,
                        Description: ls.Skill.Description,
                        Type: ls.Skill.Type.ToString(),
                        TargetType: ls.Skill.TargetType.ToString(),
                        MpCost: ls.Skill.MpCost,
                        CooldownTurns: ls.Skill.CooldownTurns,
                        CurrentCooldown: ls.CurrentCooldown,
                        UnlockLevel: ls.Skill.UnlockLevel
                    )).ToList()
                );

                return Results.Ok(dto);
            })
            .WithName("GetCharacterStats");

            // POST /characters/{id}/allocate-stats — spend skill points
            group.MapPost("/{id:guid}/allocate-stats", async (
                Guid id,
                AllocateStatsRequest request,
                ICharacterProgressionService progression,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                var stats = await progression.AllocateStatsAsync(
                    id, playerId,
                    request.Hp, request.Mp, request.Attack,
                    request.Defense, request.MagicPower, request.Speed);

                return Results.Ok(new
                {
                    message = "Stats allocated successfully.",
                    stats = new
                    {
                        stats.TotalHp,
                        stats.TotalMp,
                        stats.TotalAttack,
                        stats.TotalDefense,
                        stats.TotalMagicPower,
                        stats.TotalSpeed
                    }
                });
            })
            .WithName("AllocateStats");
        }

        private static CharacterDto MapToDto(Models.Game.Character c, ICharacterProgressionService progression)
        {
            return new CharacterDto(
                Id: c.Id,
                Name: c.Name,
                CharacterClass: c.Class.ToString(),
                Level: c.Level,
                Experience: c.Experience,
                ExperienceToNextLevel: c.Level < 50 ? progression.GetXpForLevel(c.Level + 1) : 0,
                SkillPoints: c.SkillPoints,
                CurrentHp: c.CurrentHp,
                CurrentMp: c.CurrentMp,
                Gold: c.Gold,
                CreatedAt: c.CreatedAt
            );
        }
    }
}
