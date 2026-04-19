using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A status effect currently active on a battle participant.
    /// Processed each turn by the StatusEffectProcessor.
    /// </summary>
    public class ActiveStatusEffect
    {
        public Guid Id { get; set; }
        public Guid BattleId { get; set; }
        public Battle Battle { get; set; } = null!;

        public Guid TargetParticipantId { get; set; }
        public BattleParticipant TargetParticipant { get; set; } = null!;

        public StatusEffectType Type { get; set; }

        /// <summary>Remaining turns before this effect expires.</summary>
        public int RemainingTurns { get; set; }

        /// <summary>Damage or healing applied each turn tick.</summary>
        public int TickValue { get; set; }

        /// <summary>Current stack count (up to stack limit defined on skill).</summary>
        public int Stacks { get; set; } = 1;
    }
}
