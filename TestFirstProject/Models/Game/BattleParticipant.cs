using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A combatant in a battle — either the player character or an enemy.
    /// Tracks current HP/MP and turn-order position.
    /// </summary>
    public class BattleParticipant
    {
        public Guid Id { get; set; }
        public Guid BattleId { get; set; }
        public Battle Battle { get; set; } = null!;

        public ParticipantType Type { get; set; }

        // For player participants
        public Guid? CharacterId { get; set; }
        public Character? Character { get; set; }

        // For enemy participants
        public Guid? EnemyId { get; set; }
        public Enemy? Enemy { get; set; }

        // Display name for battle log
        public string Name { get; set; } = null!;

        // Current combat stats (snapshot at battle start, modified by effects)
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int CurrentMp { get; set; }
        public int MaxMp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }

        public bool IsAlive => CurrentHp > 0;

        /// <summary>Turn order position (lower = goes first).</summary>
        public int TurnOrder { get; set; }

        /// <summary>Whether this is a boss in phase 2 (below 50% HP).</summary>
        public bool IsPhaseTwo { get; set; } = false;
    }
}
