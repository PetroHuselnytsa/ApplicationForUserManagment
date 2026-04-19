using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Rolls loot from enemy loot tables and awards items to characters.
    /// </summary>
    public interface ILootService
    {
        /// <summary>Roll loot drops from a defeated enemy's loot table.</summary>
        Task<List<LootDropResponse>> RollLootAsync(Guid enemyId, Guid characterId);
    }
}
