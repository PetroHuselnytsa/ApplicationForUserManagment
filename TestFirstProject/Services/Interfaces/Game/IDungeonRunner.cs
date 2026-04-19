using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Manages dungeon runs: creation, room generation, progression, and completion.
    /// </summary>
    public interface IDungeonRunner
    {
        /// <summary>Start a new dungeon run in a zone.</summary>
        Task<DungeonRun> StartDungeonAsync(Guid characterId, Guid zoneId);

        /// <summary>Get the current state of a dungeon run.</summary>
        Task<DungeonRun> GetDungeonRunAsync(Guid runId);

        /// <summary>Perform an action in the current room (enter, interact, proceed).</summary>
        Task<DungeonRoomResult> ActInRoomAsync(Guid runId, Guid characterId, string action);
    }

    public class DungeonRoomResult
    {
        public string RoomType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
        public Guid? BattleId { get; set; }
        public int? TrapDamage { get; set; }
        public int? HpRestored { get; set; }
        public int? MpRestored { get; set; }
        public string? TreasureItemName { get; set; }
        public bool IsDungeonComplete { get; set; }
    }
}
