using TestFirstProject.DTOs.Game;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Endpoints.Game
{
    /// <summary>
    /// Maps inventory endpoints: list items, equip, use consumable, enchant, stash.
    /// All endpoints require JWT authentication.
    /// </summary>
    public static class InventoryEndpoints
    {
        public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
        {
            var charGroup = app.MapGroup("/api/characters/{characterId}/inventory")
                               .WithTags("Inventory")
                               .RequireAuthorization();

            // GET /characters/{characterId}/inventory — list items
            charGroup.MapGet("/", async (Guid characterId, IInventoryService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.GetInventoryAsync(playerId, characterId);
                return Results.Ok(result);
            })
            .WithName("GetInventory");

            // POST /characters/{characterId}/inventory/equip — equip an item
            charGroup.MapPost("/equip", async (Guid characterId, EquipItemRequest request, IInventoryService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.EquipItemAsync(playerId, characterId, request);
                return Results.Ok(result);
            })
            .WithName("EquipItem");

            // POST /characters/{characterId}/inventory/use — use a consumable
            charGroup.MapPost("/use", async (Guid characterId, UseItemRequest request, IInventoryService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.UseItemAsync(playerId, characterId, request);
                return Results.Ok(new { message = result });
            })
            .WithName("UseItem");

            // POST /characters/{characterId}/inventory/enchant — enchant an item
            charGroup.MapPost("/enchant", async (Guid characterId, EnchantItemRequest request, IInventoryService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.EnchantItemAsync(playerId, characterId, request);
                return Results.Ok(result);
            })
            .WithName("EnchantItem");

            // GET /players/stash — shared stash
            app.MapGet("/api/players/stash", async (IInventoryService service, HttpContext ctx) =>
            {
                var playerId = ctx.GetUserId();
                var result = await service.GetStashAsync(playerId);
                return Results.Ok(result);
            })
            .WithTags("Inventory")
            .RequireAuthorization()
            .WithName("GetStash");
        }
    }
}
