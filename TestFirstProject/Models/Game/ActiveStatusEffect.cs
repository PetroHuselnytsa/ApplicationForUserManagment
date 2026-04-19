using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A status effect currently active on a battle participant.
    /// </summary>
    public class ActiveStatusEffect
    {
        public Guid Id { get; set; }
        public Guid BattleParticipantId { get; set; }
        public StatusEffectType Type { get; set; }

        /// <summary>Remaining turns before the effect expires.</summary>
        public int RemainingTurns { get; set; }

        /// <summary>Damage or healing applied per turn tick.</summary>
        public int TickValue { get; set; }

        /// <summary>Current stack count (up to the effect's stack limit).</summary>
        public int StackCount { get; set; } = 1;

        // Navigation
        public BattleParticipant BattleParticipant { get; set; } = null!;
    }
}
