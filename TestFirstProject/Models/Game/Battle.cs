using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Represents a persisted battle instance. Combat state is fully stored in DB
    /// so players can disconnect and resume.
    /// </summary>
    public class Battle
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public BattleStatus Status { get; set; } = BattleStatus.InProgress;
        public int CurrentTurn { get; set; } = 1;

        /// <summary>Index into the turn order list indicating whose turn it is.</summary>
        public int CurrentTurnIndex { get; set; } = 0;

        // Optional link to dungeon room that spawned this battle
        public Guid? DungeonRoomId { get; set; }

        // Rewards (populated on victory)
        public int XpReward { get; set; }
        public int GoldReward { get; set; }

        /// <summary>Optimistic concurrency token to prevent concurrent action submissions.</summary>
        public uint RowVersion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
        public ICollection<ActiveStatusEffect> StatusEffects { get; set; } = new List<ActiveStatusEffect>();
    }
}
