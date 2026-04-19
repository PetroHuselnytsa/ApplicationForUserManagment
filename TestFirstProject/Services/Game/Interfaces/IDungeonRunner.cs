using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Dungeon run management: start runs, navigate rooms, handle encounters.
    /// </summary>
    public interface IDungeonRunner
    {
        /// <summary>Start a new dungeon run in a zone.</summary>
        Task<DungeonRunResponse> StartRunAsync(Guid playerId, Guid characterId, Guid zoneId);

        /// <summary>Get the current state of a dungeon run.</summary>
        Task<DungeonRunResponse> GetRunStateAsync(Guid playerId, Guid runId);

        /// <summary>Act in the current room (enter, continue, flee).</summary>
        Task<DungeonRunResponse> ActInRoomAsync(Guid playerId, Guid runId, DungeonActionRequest request);
    }
}
