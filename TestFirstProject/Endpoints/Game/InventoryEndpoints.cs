using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Inventory, equipment, consumable, enchanting, and stash endpoints.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class InventoryEndpoints
    {
        public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
        {
            // --- Character Inventory ---
            var charGroup = app.MapGroup("/api/characters/{characterId:guid}/inventory")
                               .WithTags("Inventory")
                               .RequireAuthorization();

            // GET /characters/{id}/inventory — list items
            charGroup.MapGet("/", async (
                Guid characterId,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var items = await lootService.GetInventoryAsync(characterId);

                var dtos = items.Select(ii => new InventoryItemDto(
                    Id: ii.Id,
                    ItemId: ii.ItemId,
                    Name: ii.Item.Name,
                    Description: ii.Item.Description,
                    Type: ii.Item.Type.ToString(),
                    Rarity: ii.Item.Rarity.ToString(),
                    Quantity: ii.Quantity,
                    EnchantmentLevel: ii.EnchantmentLevel,
                    LevelRequirement: ii.Item.LevelRequirement,
                    ClassRestriction: ii.Item.ClassRestriction?.ToString(),
                    Stats: new ItemStatsDto(
                        BonusHp: ii.Item.BonusHp,
                        BonusMp: ii.Item.BonusMp,
                        BonusAttack: ii.Item.BonusAttack,
                        BonusDefense: ii.Item.BonusDefense,
                        BonusMagicPower: ii.Item.BonusMagicPower,
                        BonusSpeed: ii.Item.BonusSpeed,
                        BonusCritChance: ii.Item.BonusCritChance,
                        BonusDodgeChance: ii.Item.BonusDodgeChance,
                        EnchantBonusAttack: ii.EnchantBonusAttack,
                        EnchantBonusDefense: ii.EnchantBonusDefense,
                        EnchantBonusHp: ii.EnchantBonusHp,
                        EnchantBonusMagicPower: ii.EnchantBonusMagicPower
                    )
                )).ToList();

                return Results.Ok(dtos);
            })
            .WithName("GetInventory");

            // POST /characters/{id}/inventory/equip — equip an item
            charGroup.MapPost("/equip", async (
                Guid characterId,
                EquipItemRequest request,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                await lootService.EquipItemAsync(characterId, request.InventoryItemId);
                return Results.Ok(new { message = "Item equipped successfully." });
            })
            .WithName("EquipItem");

            // POST /characters/{id}/inventory/use — use a consumable
            charGroup.MapPost("/use", async (
                Guid characterId,
                UseItemRequest request,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var result = await lootService.UseConsumableAsync(characterId, request.InventoryItemId);
                var dto = new ConsumableUseResultDto(
                    HpRestored: result.HpRestored,
                    MpRestored: result.MpRestored,
                    BuffApplied: result.BuffApplied,
                    Message: result.Message
                );
                return Results.Ok(dto);
            })
            .WithName("UseConsumable");

            // POST /characters/{id}/inventory/enchant — enchant an item
            charGroup.MapPost("/enchant", async (
                Guid characterId,
                EnchantItemRequest request,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var invItem = await lootService.EnchantItemAsync(characterId, request.InventoryItemId);
                return Results.Ok(new
                {
                    message = $"Enchantment successful! Item is now +{invItem.EnchantmentLevel}.",
                    enchantmentLevel = invItem.EnchantmentLevel
                });
            })
            .WithName("EnchantItem");

            // --- Shared Stash ---
            var stashGroup = app.MapGroup("/api/players/stash")
                                .WithTags("Stash")
                                .RequireAuthorization();

            // GET /players/stash — view shared stash
            stashGroup.MapGet("/", async (
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                var stash = await lootService.GetStashAsync(playerId);

                var dtos = stash.Select(s => new StashItemDto(
                    Id: s.Id,
                    ItemId: s.ItemId,
                    Name: s.Item.Name,
                    Type: s.Item.Type.ToString(),
                    Rarity: s.Item.Rarity.ToString(),
                    Quantity: s.Quantity
                )).ToList();

                return Results.Ok(dtos);
            })
            .WithName("GetStash");

            // Move to stash from character inventory
            charGroup.MapPost("/stash", async (
                Guid characterId,
                MoveToStashRequest request,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                await lootService.MoveToStashAsync(characterId, playerId, request.InventoryItemId, request.Quantity);
                return Results.Ok(new { message = "Item moved to stash." });
            })
            .WithName("MoveToStash");

            // Move from stash to character inventory
            stashGroup.MapPost("/withdraw", async (
                MoveFromStashRequest request,
                ILootService lootService,
                HttpContext httpContext) =>
            {
                var playerId = httpContext.GetUserId();
                await lootService.MoveFromStashAsync(playerId, request.CharacterId, request.StashId, request.Quantity);
                return Results.Ok(new { message = "Item moved from stash to inventory." });
            })
            .WithName("WithdrawFromStash");
        }
    }
}
