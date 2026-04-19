using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Immutable log entry for a single turn action in a battle.
    /// </summary>
    public class BattleTurnLog
    {
        public Guid Id { get; set; }
        public Guid BattleId { get; set; }
        public int TurnNumber { get; set; }
        public Guid ActorId { get; set; }
        public Guid? TargetId { get; set; }
        public BattleAction Action { get; set; }

        /// <summary>Skill name or item name used, if applicable.</summary>
        public string? ActionDetail { get; set; }

        public int DamageDealt { get; set; }
        public int HealingDone { get; set; }
        public bool WasCritical { get; set; }
        public bool WasDodged { get; set; }
        public string? StatusEffectApplied { get; set; }
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation
        public Battle Battle { get; set; } = null!;
    }
}
