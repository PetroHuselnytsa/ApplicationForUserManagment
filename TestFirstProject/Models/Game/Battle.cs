using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Represents a persisted combat encounter.
    /// Combat state is stored in DB so players can disconnect and resume.
    /// </summary>
    public class Battle
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }

        /// <summary>Optional link to a dungeon room that spawned this battle.</summary>
        public Guid? DungeonRoomId { get; set; }

        public BattleStatus Status { get; set; } = BattleStatus.InProgress;
        public int CurrentTurn { get; set; } = 1;

        /// <summary>ID of the participant whose turn it currently is.</summary>
        public Guid? CurrentTurnParticipantId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        /// <summary>Optimistic concurrency token to prevent duplicate turn submissions.</summary>
        public uint RowVersion { get; set; }

        // Navigation
        public Character Character { get; set; } = null!;
        public DungeonRoom? DungeonRoom { get; set; }
        public ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
        public ICollection<BattleTurnLog> TurnLogs { get; set; } = new List<BattleTurnLog>();
    }
}
