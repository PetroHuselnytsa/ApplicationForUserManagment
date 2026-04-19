namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A participant in a battle — either the player's character or an enemy.
    /// Stores current combat HP/MP and status effects.
    /// </summary>
    public class BattleParticipant
    {
        public Guid Id { get; set; }
        public Guid BattleId { get; set; }

        /// <summary>If this participant is a player character.</summary>
        public Guid? CharacterId { get; set; }

        /// <summary>If this participant is an enemy.</summary>
        public Guid? EnemyId { get; set; }

        public bool IsPlayer { get; set; }

        public string Name { get; set; } = null!;
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMP { get; set; }
        public int MaxMP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }
        public bool IsAlive { get; set; } = true;

        /// <summary>For boss enemies: tracks whether the boss has entered phase 2.</summary>
        public bool IsPhase2 { get; set; }

        // Navigation
        public Battle Battle { get; set; } = null!;
        public Character? Character { get; set; }
        public Enemy? Enemy { get; set; }
        public ICollection<ActiveStatusEffect> StatusEffects { get; set; } = new List<ActiveStatusEffect>();
    }
}
