using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Core combat engine responsible for the full turn-based battle loop.
    /// All battle state is persisted in the database for disconnect/resume.
    /// </summary>
    public interface ICombatEngine
    {
        /// <summary>Start a new battle against a specific enemy.</summary>
        Task<Battle> StartBattleAsync(Guid characterId, Guid enemyId, Guid? dungeonRoomId = null);

        /// <summary>Get the current state of a battle.</summary>
        Task<Battle> GetBattleStateAsync(Guid battleId);

        /// <summary>Submit a player's action for the current turn and process the full turn cycle.</summary>
        Task<BattleTurnResult> SubmitActionAsync(Guid battleId, Guid playerId, BattleActionType actionType, Guid? skillId = null, Guid? itemId = null, Guid? targetId = null);
    }

    /// <summary>
    /// Result of processing a full turn cycle (player action + enemy actions).
    /// </summary>
    public class BattleTurnResult
    {
        public Guid BattleId { get; set; }
        public BattleStatus Status { get; set; }
        public int TurnNumber { get; set; }
        public List<BattleLogEntry> Log { get; set; } = new();
        public List<DroppedItem>? Loot { get; set; }
        public int XpAwarded { get; set; }
        public int GoldAwarded { get; set; }
    }

    public class BattleLogEntry
    {
        public string Actor { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? Target { get; set; }
        public int? Damage { get; set; }
        public int? Healing { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDodged { get; set; }
        public string? StatusEffect { get; set; }
        public string? Message { get; set; }
    }

    public class DroppedItem
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
